let canvas;
let ctx;
let stationWidth;
let stationHeight;
let stations = []; // Global array to store station data


// Transformation parameters for the background image
let bgOffsetX = 0;
let bgOffsetY = 0;
let bgScaleFactor = 1;
const background = new Image();

let mouseDown = 0;
let isClickedOnStation = false;
let dragStation = -1;
let selectedStationInfo = -1;
let cancelClick = false;
let dragging = 0;
const DRAGDELAY = 300;
const SCREEN_WIDTH = screen.width;
const SCREEN_HEIGHT = screen.height;
const PINGDELAY = 1000; // ping stations every 1 sec
const STATION_GRID_WIDTH = 1920;
const STATION_GRID_HEIGHT = 1080; //this is how station coordinates are stored in sql - 1920x1080 grid

const SCALE_W = SCREEN_WIDTH / STATION_GRID_WIDTH;
const SCALE_H = SCREEN_HEIGHT / STATION_GRID_HEIGHT;
let prevDrawWidth;
let prevDrawHeight;

let changeInDrawWidth = 0;
let changeInDrawHeight = 0;

const colors = [ // color stops
    { position: 0, color: [255, 28, 43] },       // Red
    { position: 0.25, color: [252, 27, 106] },        //pink
    { position: 0.5, color: [248, 255, 38] },   // Yellow
    { position: 0.75, color: [64, 217, 241] },    // blue
    { position: 1, color: [70, 228, 86] }       // Green
];
let currentMouseEvent;
let editing = false;
export { stationWidth, stationHeight, stations, DRAGDELAY,dragging,cancelClick,selectedStationInfo,dragStation,mouseDown,colors };
export const setSelectedStation = (val) => { selectedStationInfo = val; };
export const setStations = (val) => { stations = val; };
export const setEditing = (val) => { editing = val; };
export async function loadUnitMapAndStations() { 
    
    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
    await setPodMap();

    await loadStations();

    setLegend();

    await hideStationInfo();
}


async function pingStations() {
    // ping and update the stations from the database.
    console.log(editing);
    var draggedX;
    var draggedY;
    var updateDrag = false;
    //while a station is being dragged, update every field BUT the station's x and y coordinates
    if (dragStation > -1 && dragging == true) {
        draggedX = stations[dragStation].Coordinates.X;
        draggedY = stations[dragStation].Coordinates.Y;
        updateDrag = true;
    }

    try {
        const response = await fetch('get-pod', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        });

        const data = await response.json();
        stations = data.HHStations;

        if (updateDrag) {
            stations[dragStation].Coordinates.X = draggedX;
            stations[dragStation].Coordinates.Y = draggedY;

        }
        //draw the stations over
        ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
        drawPodMap();
        drawStations();

        // make sure to update the highlighted station info if it is selected

        if (selectedStationInfo > 0) {
            //check if currently editing
            try {
                const response = await fetch('load-station-info-partial', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json', 'editing': editing },
                    body: JSON.stringify(selectedStationInfo)
                });
                if (!response.ok) {
                    throw new Error('Failed to load station info');
                }
                
                if (!editing) { // only if not editing
                    const partialViewHtml = await response.text();
                    document.getElementById("station-info-container").innerHTML = partialViewHtml;
                } 

                
            } catch (error) {
                console.error('Error updating station info:', error);
            }
        } 

    } catch (error) {
        console.error('Error getting pod:', error);
    }
    
}



function setLegend() {

    const legend = document.getElementById("legend");
    const gradientStops = colors.map(stop => {
        const [r, g, b] = stop.color;
        return `rgb(${r}, ${g}, ${b}) ${stop.position * 100}%`;
    });

    // Apply linear gradient background
    legend.style.background = `linear-gradient(to right, ${gradientStops.join(', ')})`;

}
export async function updateStation(stationKey) {
    try {
        const station = stations[stationKey];
        console.log(station);
        const response = await fetch('update-station', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                stations: stations,            // All stations
                stationKey: parseInt(stationKey),
                mode: "edit", // The specific station being updated
                newNotes: station.newNotes || [], // Send newNotes array for this station
                archivedNotes: station.archivedNotes || []
            })
        });
        if (!response.ok) {
            throw new Error('Failed to update');
        }
        stations[stationKey].newNotes = [];
        const partialViewHtml = await response.text();
        document.getElementById("station-info-container").innerHTML = (partialViewHtml);
        
    } catch (error) {
        console.error('Error updating station info:', error);
    }
}

export function initializeCanvas() {
    editing = false;
    // Get the canvas element and its context
    canvas = document.getElementById('podMapCanvas');
    ctx = canvas.getContext('2d');
    canvas.width = screen.width;
    canvas.height = screen.height;

    stationWidth = 30;
    stationHeight = stationWidth;

    canvas.onmousedown = function () {
        ++mouseDown;
       
    }

    canvas.onmouseleave = function () {
        if (mouseDown) {
            --mouseDown;
            /*checkIfStationDropped();*/
        }

        checkIfStationDropped();

    }

    canvas.onmouseup = function () {
        --mouseDown;
        checkIfStationDropped();
        
    }
    setInterval(() => pingStations(), PINGDELAY);

    document.body.addEventListener('mousemove', (event) => {
        currentMouseEvent = event; // always up-to-date
        if (isHoverOverStation()) {

            document.getElementById('podMapCanvas').style.cursor = dragging ? "grab" : "pointer";
        } else {
            document.getElementById('podMapCanvas').style.cursor = 'default';
        }
    });

    canvas.addEventListener('wheel', function (event) {
        if (event.ctrlKey) {
            event.preventDefault(); // Prevent page zoom

            const scaleIncrement = 0.1;
            const direction = -1 *  Math.sign(event.deltaY); // will be -1, 0, or 1
            
            bgScaleFactor += (direction * scaleIncrement);
            bgScaleFactor = Math.min(3, Math.max(0.5, bgScaleFactor));
            dragStationLoop();
        }
    }, { passive: false });

   canvas.addEventListener('click', function (event) {
        if (cancelClick) {
            event.stopPropagation();
            event.preventDefault(); // Optional: avoid default browser behavior
            cancelClick = false; // Reset for next click
            return;
        }

        
        let clicked = false;

        for (const station of Object.values(stations)) {
            if (isClickOnStation(station)) {
                showStationInfo(station.StationKey);
                dragStationLoop(); // do one more redraw to remove stroke around dragged station
                clicked = true;
                break;
            }
        }
    });
}

function checkIfStationDropped() {
    if (dragStation != null && dragStation >= 0) {
        dragging = false;
        updateStation(dragStation);
        dragStation = -1;
        //selectedStationInfo = -1;
        cancelClick = true;
        dragStationLoop();
    }
}

export function dragStationLoop() {
    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    var rect = canvas.getBoundingClientRect();

    // Compute scaling factors for the canvas drawing buffer relative to its on-screen size
    var scaleX = (canvas.width / rect.width) * bgScaleFactor;
    var scaleY = (canvas.height / rect.height) * bgScaleFactor;

   
    // Get the mouse position relative to the canvas, accounting for scaling
   /* var mouseX = (currentMouseEvent.clientX - rect.left) * scaleX;
    var mouseY = (currentMouseEvent.clientY - rect.top) * scaleY;*/
    const event = currentMouseEvent;
    let [mouseX, mouseY] = translateMouseClickToCanvasCoords(event);

    drawPodMap();

    if (dragStation >= 0) {
        const [logicalX, logicalY] = reverseTransformCoordinates(mouseX - (stationWidth / 2), mouseY - (stationHeight / 2));

        stations[dragStation].Coordinates.X = logicalX;
        stations[dragStation].Coordinates.Y = logicalY;
    }
    drawStations();
   
}
function drawStations() {
    Object.values(stations).forEach((station) => {
        drawStation(station);

    });
}
export function isMouseHoldOnStation(station) {
    if (isClickOnStation(station) && mouseDown) {
        dragStation = station.StationKey; 
        selectedStationInfo = dragStation;
        showStationInfo(selectedStationInfo);
        dragging = true;
        runDragLoop();
        document.getElementById('podMapCanvas').style.cursor = 'grab';
    }
}

function runDragLoop() {
    if (!dragging || dragStation < 0) return;
    //selectedStationInfo = -1;
    //hideStationInfo();
    dragStationLoop();
    requestAnimationFrame(() => runDragLoop()); // smooth loop at ~60fps
}

export function getInterpolatedColor(v, colorStops) {
    // Clamp v between 0 and 1
    v = Math.max(0, Math.min(1, v));

    // Find the two color stops v is between
    let start = colorStops[0];
    let end = colorStops[colorStops.length - 1];

    for (let i = 0; i < colorStops.length - 1; i++) {
        if (v >= colorStops[i].position && v <= colorStops[i + 1].position) {
            start = colorStops[i];
            end = colorStops[i + 1];
            break;
        }
    }

    // Interpolate factor between the two color stops
    const t = (v - start.position) / (end.position - start.position);

    // Linear interpolation of RGB values
    const r = Math.round(start.color[0] + t * (end.color[0] - start.color[0]));
    const g = Math.round(start.color[1] + t * (end.color[1] - start.color[1]));
    const b = Math.round(start.color[2] + t * (end.color[2] - start.color[2]));

    return `rgb(${r}, ${g}, ${b})`;
}

function darkenColor(color) {
    const match = color.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/);
    if (!match) throw new Error("Invalid RGB string format");

    const factor = 0.75;

    const r = Math.round(parseInt(match[1]) * factor);
    const g = Math.round(parseInt(match[2]) * factor);
    const b = Math.round(parseInt(match[3]) * factor);

    return `rgb(${r}, ${g}, ${b})`;
}
function modelResultToColor(v) {
    /*const colors = [ // color stops
        { position: 0, color: [255, 0, 0] },       // Red
        { position: 0.5, color: [244, 221, 9] },   // Yellow
        { position: 1, color: [99, 255, 0] }       // Green
    ];*/

    

    return getInterpolatedColor(v, colors);
}

export async function drawPodMap() {
    // Clear the canvas before redrawing
    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
/*
    const canvasWidth = ctx.canvas.width;
    const canvasHeight = ctx.canvas.height;*/

    const canvasWidth = SCREEN_WIDTH;
    const canvasHeight = SCREEN_HEIGHT;
    const imageAspectRatio = background.width / background.height;
    const canvasAspectRatio = canvasWidth / canvasHeight;
    
    let baseWidth, baseHeight;

    // Determine base dimensions (before scaling)
    if (imageAspectRatio > canvasAspectRatio) {
        baseWidth = canvasWidth;
        baseHeight = canvasWidth / imageAspectRatio;
    } else {
        baseHeight = canvasHeight;
        baseWidth = canvasHeight * imageAspectRatio;
    }

    
    // Apply the zoom scale factor
    const drawWidth = baseWidth * bgScaleFactor;
    const drawHeight = baseHeight * bgScaleFactor;
    

    // Center the zoomed image
    bgOffsetX = (canvasWidth - drawWidth) / 2;
    bgOffsetY = (canvasHeight - drawHeight) / 2;

    /*console.log("wdith: " + drawWidth + "heigfht " + drawHeight)*/

    ctx.drawImage(background, bgOffsetX, bgOffsetY, drawWidth, drawHeight);
}

function getTransformedCoordinates(x, y) {
    const centerX = SCREEN_WIDTH / 2;
    const centerY = SCREEN_HEIGHT / 2;

    

    return [centerX + (x * SCALE_W - centerX) * bgScaleFactor , centerY + (y * SCALE_H - centerY) * bgScaleFactor ]
}


function reverseTransformCoordinates(x, y) {
    const centerX = SCREEN_WIDTH / 2;
    const centerY = SCREEN_HEIGHT / 2;

    return [
        (x/SCALE_W - centerX) / bgScaleFactor + centerX,
        (y/SCALE_H - centerY) / bgScaleFactor + centerY
    ];
}

function drawStation(station) {
    const [transformedX, transformedY] = getTransformedCoordinates(station.Coordinates.X, station.Coordinates.Y);

    let transformedWidth = stationWidth * bgScaleFactor * SCALE_W;
    let transformedHeight = stationHeight * bgScaleFactor * SCALE_H;

    const fillColor = modelResultToColor(station.ModelResult);

    // Highlight if selected
    if (station.StationKey == selectedStationInfo && !dragging) {
        ctx.shadowColor = fillColor;
        ctx.shadowBlur = 15;
        ctx.shadowOffsetX = 0;
        ctx.shadowOffsetY = 0;
    }

    // Add shadow while dragging
    if (station.StationKey == dragStation) {
        ctx.shadowColor = "rgba(0, 0, 0, 0.5)";
        ctx.shadowBlur = 10;
        ctx.shadowOffsetX = 5;
        ctx.shadowOffsetY = 5;

        transformedWidth *= 1.1;
        transformedHeight *= 1.1;
    }

    // Border
    ctx.lineWidth = 8;
    ctx.strokeStyle = darkenColor(fillColor);
    ctx.strokeRect(transformedX, transformedY, transformedWidth, transformedHeight);

    // Fill
    ctx.beginPath();
    ctx.fillStyle = fillColor;
    ctx.fillRect(transformedX, transformedY, transformedWidth, transformedHeight);
    ctx.stroke();

    // Reset shadow
    ctx.shadowColor = "transparent";
    ctx.shadowBlur = 0;
    ctx.shadowOffsetX = 0;
    ctx.shadowOffsetY = 0;
}


function translateMouseClickToCanvasCoords(event){
    var rect = canvas.getBoundingClientRect();
    var computedStyle = window.getComputedStyle(canvas);

    var canvasHeight = parseFloat(computedStyle.height);
    var canvasWidth = canvasHeight * (16 / 9);// use height (since height is maxed) and known aspect ratio to calculate width


    var scaleX = ((canvas.width) / (canvasWidth));
    var scaleY = (canvas.height / (canvasHeight));

    // Get the mouse position relative to the canvas, accounting for scaling
    var mouseX = (event.clientX - rect.left) * scaleX;
    var mouseY = (event.clientY - rect.top) * scaleY;

    //return getTransformedCoordinates(mouseX, mouseY);
/*
    ctx.beginPath();
    ctx.fillStyle = 'red';
    ctx.fillRect(mouseX, mouseY, 5, 5);
    ctx.stroke();*/
    
    return [mouseX, mouseY];
}
export function isClickOnStation(station) {
    // Get the mouse event from the global context (assuming it's defined)
    const event = currentMouseEvent;
    const [mouseX, mouseY] = translateMouseClickToCanvasCoords(event);

    let [stationX, stationY] = getTransformedCoordinates(station.Coordinates.X, station.Coordinates.Y);


    return (mouseX >= stationX &&
    mouseX <= stationX + stationWidth * bgScaleFactor &&
    mouseY >= stationY &&
        mouseY <= stationY + stationHeight * bgScaleFactor);



    // Check if the mouse click is within the bounds of the station
    /*return (mouseX >= station.Coordinates.X &&
        mouseX <= station.Coordinates.X + stationWidth &&
        mouseY >= station.Coordinates.Y &&
        mouseY <= station.Coordinates.Y + stationHeight);*/
}



function isHoverOverStation() {
    const event = currentMouseEvent;
    const [mouseX, mouseY] = translateMouseClickToCanvasCoords(event);

    const hoverRadius = stationHeight * bgScaleFactor;

    for (const station of Object.values(stations)) {
        const [stationX, stationY] = getTransformedCoordinates(station.Coordinates.X, station.Coordinates.Y);

        const dx = mouseX - stationX;
        const dy = mouseY - stationY;
        const distance = Math.sqrt(dx * dx + dy * dy);

        if (distance <= hoverRadius) {
            return true;
        }
    }

    return false;
}

    

export async function loadStations() {
    
    try {
        const response = await fetch('get-pod', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        });

        const data = await response.json();
    
        // Store the station data globally

        stations = data.HHStations;
        drawStations();



    } catch (error) {
        console.error('Error getting pod:', error);
    }
}

export async function hideStationInfo() {
   
        try {
            const response = await fetch('add-station-partial');

            if (!response.ok) {
                throw new Error('Failed to load add station');
            }

            const partialViewHtml = await response.text();
            document.getElementById("station-info-container").innerHTML = partialViewHtml;
            selectedStationInfo = -1;
            dragStationLoop();


        } catch (error) {
            console.error('Error showing add station', error);
        }
   /* }*/
       /* console.log("False")
        document.getElementById("station-info-container").innerHTML = null;
        isClickedOnStation = false;
   */
        
}
export async function showStationInfo(key) {
   try {
        const response = await fetch('load-station-info-partial', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(key)
        });

        if (!response.ok) {
            throw new Error('Failed to load station info');
        }

        const partialViewHtml = await response.text();
        document.getElementById("station-info-container").innerHTML = partialViewHtml;
        selectedStationInfo = key;
        // draw stations again to show selected station stroke
        dragStationLoop();
        //console.log('updated station info', response);
    } catch (error) {
        console.error('Error updating station info:', error);
        }
    
   
}




export async function setPodMap() {
    // Clear the canvas
    //ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    try {
        const response = await fetch('/get-pod-map');
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.json();

        // Load and draw the background image only if data is available
        if (data && data.pod && data.floor) {
            
            background.src = "\\Maps\\L" + data.floor + "\\" + data.floor + data.pod + ".svg";

            background.onload = async function () {
                await drawPodMap();
               // console.log("map drawn");

            };
        }
        return data;
    } catch (error) {
        console.error('There was a problem with the fetch operation:', error);
        return;
    }
}

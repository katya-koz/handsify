
import { initializePage } from "./navigation.js";
import {cancelClick, hideStationInfo,setEditing, stationWidth, stationHeight, initializeCanvas, stations, updateStation, isClickOnStation, DRAGDELAY, isMouseHoldOnStation, dragStationLoop, setSelectedStation, loadStations } from "./canvas.js";


var tempElements = {
    "station-name-field": null,
    "station-id-field": null,
    "station-location-field": null,
    "station-type-field": null
};

const dropdownElements = {
    "station-type-field": ["Sanitizer", "Soap"],
    "station-location-field": ["Inside", "Outside", "Inside Extra", "Pillar", "Cart Area", "Equipment", "Nourishment", "Pneumatic Tube"]
};

// Onclick functions
function ToggleOnTextEditField(item) {
    setEditing(true);
    var buttonType = item.getAttribute("data-field");
    // data-field has same value as the ID of the field container
    var field = document.getElementById(buttonType);
    // Disable edit button
    item.classList.add("disabled");
    // Reveal undo and accept buttons
    var undo = document.getElementById(buttonType + "-undo");
    var accept = document.getElementById(buttonType + "-accept");
    undo.classList.remove("disabled");
    accept.classList.remove("disabled");

    // Save old element to put back later
    tempElements[buttonType] = field;

    // Replace text with an input field
    var editableText = document.createElement("input");
    editableText.classList.add("form-control");
    editableText.id = buttonType + "-input";
    var oldValue = field.innerText;

    field.replaceWith(editableText);
    editableText.value = oldValue;
}
function ToggleOnAddNoteField(item) {
    setEditing(true);
    var buttonType = item.getAttribute("data-field");
   // console.log(buttonType);
    // data-field has same value as the ID of the field container
    var field = document.getElementById(buttonType);
    // Disable edit button
    item.classList.add("disabled");
    // Reveal undo and accept buttons
    var undo = document.getElementById(buttonType + "-undo");
    var accept = document.getElementById(buttonType + "-accept");
    undo.classList.remove("disabled");
    accept.classList.remove("disabled");

    // Save old element to put back later
    tempElements[buttonType] = field;

    // Replace text with an input field
    var editableText = document.createElement("textarea");
    editableText.classList.add("form-control");
    editableText.id = buttonType + "-input";

    field.replaceWith(editableText);
    /*editableText.value = oldValue;*/
}

async function SaveEdit(item) {
    
    var buttonType = item.getAttribute("data-field");
    var stationKey = item.getAttribute("data-station-key");
    // Hide undo and accept buttons, and reveal edit button
    var edit = document.getElementById(buttonType + "-edit");
    var undo = document.getElementById(buttonType + "-undo");
    edit.classList.remove("disabled");
    undo.classList.add("disabled");
    item.classList.add("disabled");

    // Get current input field and determine the new value
    var currentInput = document.getElementById(buttonType + "-input");
    var newInputValue;
    if (currentInput.firstChild != null) {
        // This indicates the input is a dropdown type;
        // get the innerText from the first child element
        newInputValue = currentInput.firstChild.innerText;
    } else {
        newInputValue = currentInput.value;
    }

    // Replace with stored previous field and update its value
    currentInput.replaceWith(tempElements[buttonType]);
    var field = document.getElementById(buttonType);
    //field.innerText = newInputValue;
    await updateStationsList(stationKey, buttonType, newInputValue);
    await updateStation(stationKey);
    setEditing(false);
}

function CancelEdit(item) {
    var buttonType = item.getAttribute("data-field");

    // Hide undo and accept buttons, and reveal edit button
    var edit = document.getElementById(buttonType + "-edit");
    var accept = document.getElementById(buttonType + "-accept");
    edit.classList.remove("disabled");
    accept.classList.add("disabled");
    item.classList.add("disabled");

    // Get current input field and restore the old element
    var currentInput = document.getElementById(buttonType + "-input");
    currentInput.replaceWith(tempElements[buttonType]);
    console.log("cancelled update");
    setEditing(false);
}

function ToggleOnDropDownEditField(item) {
    setEditing(true);
    var buttonType = item.getAttribute("data-field");
    // data-field has same value as the ID of the field container
    var field = document.getElementById(buttonType);
    // Disable edit button
    item.classList.add("disabled");
    // Reveal undo and accept buttons
    var undo = document.getElementById(buttonType + "-undo");
    var accept = document.getElementById(buttonType + "-accept");
    undo.classList.remove("disabled");
    accept.classList.remove("disabled");

    // Save old element to put back later
    tempElements[buttonType] = field;

    // Replace text with a dropdown input field
    var dropdownContainer = document.createElement("div");
    var dropdownToggle = document.createElement("button");

    dropdownContainer.classList.add("dropdown");
    dropdownContainer.id = buttonType + "-input";

    dropdownToggle.classList.add("btn", "btn-secondary", "dropdown-toggle");
    dropdownToggle.type = "button";
    dropdownToggle.setAttribute("data-toggle", "dropdown");
    dropdownToggle.ariaHasPopup = "true";
    dropdownToggle.innerText = field.innerText;
    dropdownToggle.ariaExpanded = "false";

    var dropdownMenu = document.createElement("div");
    dropdownMenu.classList.add("dropdown-menu");
    dropdownMenu.setAttribute("aria-label", buttonType + "-dropdown");

    dropdownContainer.appendChild(dropdownToggle);
    dropdownContainer.appendChild(dropdownMenu);

    // Populate dropdown with options
    dropdownElements[buttonType].forEach((op) => {
        var opElement = document.createElement("a");
        opElement.classList.add("dropdown-item");
        opElement.innerText = op;
        opElement.addEventListener("click", () => {
            dropdownToggle.innerText = op;
        });
        dropdownMenu.appendChild(opElement);
    });

    // Replace the field with the new dropdown container
    field.replaceWith(dropdownContainer);
}

function ArchiveNote(item) {
    var noteParentId = item.getAttribute("data-note-id");
    var noteStationKey = item.getAttribute("data-station-key");
    var noteKey = item.getAttribute("data-note-key");

    if (!stations[noteStationKey].archivedNotes) {
        stations[noteStationKey].archivedNotes = [];
    }
    stations[noteStationKey].archivedNotes.push(noteKey);
    delete stations[noteStationKey].Notes[noteKey];
    
    updateStation(noteStationKey);
}


async function updateStationsList(stationKey, editType, value) {
    // update client side
    switch (editType) {
        case "station-name-field":
            stations[stationKey].StationName = value;
            break;
        case "station-id-field":
            stations[stationKey].StationID = value;
            break;
        case "station-location-field":
            stations[stationKey].Location = value;
            break;
        case "station-type-field":
            stations[stationKey].StationType = value;
            break;
        case "note-field":
            if (!stations[stationKey].newNotes) {
                stations[stationKey].newNotes = [];
            }

            stations[stationKey].newNotes.push({
                noteKey: -1, 
                author: "",
                content: value
            });

            loadStations();
/*
            console.log(stations);*/
            break;
        default:
            break;
    }

    
}

async function ArchiveStation(item) {
    var stationKey = item.getAttribute("data-field");
    try {
        const response = await fetch('archive-station', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(stationKey)
        });
        await loadStations();
        dragStationLoop();
        setSelectedStation(-1);
        hideStationInfo();
    } catch (error) {
        console.error('Error:', error);
    }
}

// Attach a single event delegation listener to the parent container
document.addEventListener("DOMContentLoaded", function () {
    //initialize

    initializePage();
    initializeCanvas();

    const canvas = document.getElementById("podMapCanvas");
    canvas.addEventListener('mousedown', function (event) {

        for (const station of Object.values(stations)) {
            if (isClickOnStation(station)) {
                setTimeout(() => isMouseHoldOnStation(station, event), DRAGDELAY);
                break; 
            }
        }
    });

    var container = document.getElementById("station-info-container");
    if (!container) {
        console.error("Container with id 'station-info-container' not found.");
        return;
    }

    container.addEventListener("click", function (e) {
        // Check for edit-btn click
        var editBtn = e.target.closest(".edit-btn");
        if (editBtn) {
            ToggleOnTextEditField(editBtn);
            return;
        }
        var editNoteBtn = e.target.closest(".note-edit-btn");
        
        if (editNoteBtn) {
            ToggleOnAddNoteField(editNoteBtn);
            return;
        }
        // Check for undo-btn click
        var undoBtn = e.target.closest(".undo-btn");
        if (undoBtn) {
            CancelEdit(undoBtn);
            return;
        }
        // Check for accept-btn click
        var acceptBtn = e.target.closest(".accept-btn");
        if (acceptBtn) {
            SaveEdit(acceptBtn);
            return;
        }
        // Check for dropdown-edit-btn click
        var dropdownEditBtn = e.target.closest(".dropdown-edit-btn");
        if (dropdownEditBtn) {
            ToggleOnDropDownEditField(dropdownEditBtn);
            return;
        }
        // Check for archive-btn click
        var archiveBtn = e.target.closest(".archive-btn");
        if (archiveBtn) {
            ArchiveNote(archiveBtn);
            return;
        }

        var archiveStationBtn = e.target.closest(".archive-station-btn");
        if (archiveStationBtn) {
            ArchiveStation(archiveStationBtn);
            return;
        }
    });



    document.addEventListener('submit', function (event) {
        const form = event.target;

        if (form && form.id === 'station-form') {
            submitStationForm(event);
        }
    });



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
                clicked = true;
                break;
            }
        }

        if (!clicked) {
            hideStationInfo();
        }
    });

    
});
async function submitStationForm(event) {
        event.preventDefault();

        const newStation = {
            StationName: document.getElementById('station-name-input').value,
            StationID: document.getElementById('station-id-input').value,
            Location: document.getElementById('station-location-select').value,
            StationType: document.getElementById('station-type-select').value,
            StationKey: -1,
            ModelResult: 1,
            Coordinates: {
                X: stationWidth + 10,
                Y: stationHeight + 10
            }
        };

        

        try {
            const response = await fetch('add-station', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newStation)
            });

            const partialViewHtml = await response.text();

            await loadStations();


            setSelectedStation( Object.values(stations).find(station => station.StationID === Number(newStation.StationID)).StationKey);
           
            document.getElementById("station-info-container").innerHTML = partialViewHtml;
            dragStationLoop();
        } catch (error) {
            console.error('Error:', error);
        }
    }
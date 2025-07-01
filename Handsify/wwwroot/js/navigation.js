
import { loadUnitMapAndStations, loadStations, setSelectedStation } from "./canvas.js"
import { GetPodStations } from './operationmode.js';



/*Click Handlers*/
/*
export function initializeCanvas() {
    // Get the canvas element and its context
    var canvas = document.getElementById('podMapCanvas');
    var ctx = canvas.getContext('2d');

    canvas.width = screen.width;
    canvas.height = screen.height;

    stationWidth = 250;
    stationHeight = stationWidth;
}
*/




export async function initializePage() {
    var nav = await getNavigation();
    if (nav.selectedFloor) await initializeFloor(nav.selectedFloor);

    //await initializeUnit(nav.selectedUnit);
}



async function showMode(item) {
    document.getElementById("mode-dropdown").innerHTML = item;

    //var modeIndex = item.getAttribute('data-index');
    await updateSelectedIndex(item, "mode");
    //console.log(item);
    await redirectPageToSelectedMode(item);

    /* var nav = await getNavigation();
 
     await initializeFloor(nav.selectedFloor);
 
     await initializeUnit(nav.selectedUnit);
     */
}

async function showFloor(item) {
    document.getElementById("floor-dropdown").innerHTML = item;
    //var floorIndex = item.getAttribute('data-index');
    


    await updateSelectedIndex(item, "floor");
    var nav = await getNavigation();
    if(nav.selectedUnit) await initializeUnit(nav.selectedUnit);

    
}

async function showUnit(item) {
    setSelectedStation(-1); // must reset selected station once unit is switched
    
    document.getElementById("unit-dropdown").innerHTML = item;
    await updateSelectedIndex(item, "unit");
    var operation = false;
    if (document.getElementById("mode-dropdown").innerHTML.includes("Operation")) operation = true;

    if (!operation) {

        await loadUnitMapAndStations();

    } else {
        await GetPodStations();
    }
}
async function getNavigation() {
    try {
        const response = await fetch('/get-current-navigation');
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.json();
        return data;
    } catch (error) {
        console.error('There was a problem with the navigation fetch operation:', error);
        return [];
    }
}

async function initializeFloor(floor) {

    await showFloor(floor);

    
}

async function initializeUnit(unit) {
   // console.log("initializing unit: " + unit);
    // some floors dont have all units available for HH (L2 DEF), so when the floor is switched, the unit should default to the first available one
    var units = document.querySelectorAll("#unit-dropdown-items .dropdown-item");

    var availableUnits = await getAvailableUnits();
        // cycle through units, and mark non-available units
        var firstAvailible = null;

   // console.log(availableUnits);
        units.forEach((u) => {
            if (availableUnits.includes(u.innerHTML.trim())) {
                 if (firstAvailible == null) {
                    firstAvailible = u;
                } 
                u.classList.remove("disabled");
                
            } else {
                u.classList.add("disabled");
            }
        });

        await showUnit(unit);

}


async function getAvailableUnits() {
    try {
        const response = await fetch('/get-units');
        if (!response.ok) {
            throw new Error('Network response was not ok');
        }
        const data = await response.json();
        //console.log('Available units:', data);
        return data;
    } catch (error) {
        console.error('There was a problem with the fetch operation:', error);
        return [];  // Return an empty array in case of an error to avoid further issues
    }
}

async function redirectPageToSelectedMode(modeName) {
    // redirect 
    window.location.href = modeName + "-mode";
}


async function updateSelectedIndex(item, menuIdentity) {
   // console.log("index: " + item + "\t menuIdentity: " + menuIdentity);

    await fetch('update-selected-drop-menu-index', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ item: item, menuIdentity: menuIdentity })
    })
        .then(response => response.json())
        .then(data => {
            //there is a chance that the selected unit has changed (if you were on a floor that had unit A, but switched to a floor that didnt ahve unit A)
            // this is updated in the backend, but we need to reflect the new navbar changes int he front end

            document.getElementById("unit-dropdown").innerText = data.updatedUnit;

            //console.log('Model updated:', data);
        })
        .catch((error) => {
            console.error('Error updating model:', error);
        });
}

/*globalize functions */
window.showMode = showMode;
window.showFloor = showFloor;
window.showUnit = showUnit;
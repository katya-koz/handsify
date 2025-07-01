import { initializePage } from "./navigation.js";
import { stations, setStations, colors, getInterpolatedColor } from "./canvas.js";

export async function GetPodStations() {
    try {
        const response = await fetch('get-operational-pod', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        });

        const data = await response.json();

        // Store the station data globally
        setStations(data.HHStations);
        loadOperationStations();


    } catch (error) {
        console.error('Error getting pod:', error);
    }


}
 function setStationStatus(stationName, status) {
    const button = document.getElementById(`status-btn-${stationName}`);

    console.log(`Station: ${stationName}, Status: ${status}`);

    if (button) button.textContent = status;
}

async function CancelEdit(stationKey) {
    /*document.getElementById(`notes-${item}`).value = "";
    const statusBtn = document.querySelector(`#status-btn-${item}`);
    if (statusBtn) {
        statusBtn.textContent = "Set Status";
    }*/

    const station = stations[stationKey];

    const stationBox = await loadOperationViewForStation(station);

    document.getElementById(`station-box-${stationKey}`).innerHTML = stationBox.innerHTML;
}

async function loadOperationStations() {

    
    const canvasWidth = 1920;
    const canvasHeight = 1080;

    const centerX = canvasWidth / 2;
    const centerY = canvasHeight; // Entrance is at bottom center

    // Sort stations by ascending distance from entrance
    const stationsSorted = Object.values(stations).sort((a, b) => {
        const distA = Math.pow(a.Coordinates.X - centerX, 2) + Math.pow(a.Coordinates.Y - centerY, 2);
        const distB = Math.pow(b.Coordinates.X - centerX, 2) + Math.pow(b.Coordinates.Y - centerY, 2);
        return distA - distB;
    });


    // Clear container only once at the beginning
    const container = document.getElementById("stations-container");
    container.innerHTML = "";

    // Load each station in order, one at a time
    for (const station of stationsSorted) {
       const wrapper = await loadOperationViewForStation(station);
       container.appendChild(wrapper);
    }
}

// Async function to load and append one station
async function loadOperationViewForStation(station) {
    try {
        const response = await fetch(`load-operation-station-partial?stationKey=${station.StationKey}`);
        const html = await response.text();
        const wrapper = document.createElement("div");
        wrapper.innerHTML = html;

        const modelColor = getInterpolatedColor(station.ModelResult,colors);
        wrapper.firstElementChild.style.border = `0.5em solid ${modelColor}`;
        // If the partial contains a single root element:
        return wrapper.firstElementChild;
       
    } catch (err) {
        console.error(`Error loading station ${station.StationKey}:`, err);
    }
}
async function SaveEdit(station) {
    console.trace();
    const key = station.StationKey;
    const value = document.getElementById(`notes-${key}`).value;
    const statusButton = document.getElementById(`status-btn-${key}`);/*
    const statusValue = statusButton.textContent.trim(); // or `.innerText` if you prefer
*/
    if (value.trim() != "" && value.trim() != null) {
        if (!station.newNotes) {
            station.newNotes = [];
        }

        station.newNotes.push({
            noteKey: -1,
            author: "",
            content: value
        });

    }

    if (statusButton.innerText != 'Set Status') {
        const selectedText = statusButton.innerText.trim();

        // Find the matching dropdown item with the same text
        const dropdownItems = document.querySelectorAll(`#status-btn-${key} ~ .dropdown-menu .dropdown-item`);
        let selectedItem = Array.from(dropdownItems).find(el => el.innerText.trim() === selectedText);

        // Safely access its data attribute
        if (selectedItem && selectedItem.hasAttribute("data-model-result-value")) {
            const resultValue = selectedItem.getAttribute("data-model-result-value");
            stations[key].ModelResult = resultValue;
        }
    } 

    console.log(station.newNotes);

    try {
        const response = await fetch('update-station', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json'},
            body: JSON.stringify({
                stations: stations,            // All stations
                stationKey: parseInt(key),
                mode: "operation", // The specific station being updated
                newNotes: station.newNotes || [], // Send newNotes array for this station
                archivedNotes: station.archivedNotes || []
            })
        });
        if (!response.ok) {
            throw new Error('Failed to update');
        }
        stations[key].newNotes = [];
        const container = document.getElementById("station-box-" + key);
        if (station.ModelResult == 0 || station.ModelResult == 1) { container.remove() } else {

            const stationBox = await loadOperationViewForStation(station);

            document.getElementById(`station-box-${key}`).innerHTML = stationBox.innerHTML;

        };
    } catch (error) {
        console.error('Error updating station info:', error);
    }

}


document.addEventListener("DOMContentLoaded", () => {
    initializePage();
    GetPodStations();

    document.addEventListener("click", (e) => {
        const cancelBtn = e.target.closest(".cancel-btn");
        if (cancelBtn) {
            const key = cancelBtn.getAttribute("data-stationKey");
            CancelEdit(key);
        }

        const confirmBtn = e.target.closest(".confirm-btn");
        if (confirmBtn) {
            const key = confirmBtn.getAttribute("data-stationKey");
            const station = stations[key];
            SaveEdit(station);
        }
    });

    // Move this outside so it's not nested
    document.addEventListener("click", function (e) {
        const clickedItem = e.target.closest(".dropdown-item");
        if (!clickedItem) return;

        e.preventDefault();

        const dropdown = clickedItem.closest(".dropdown");
        if (!dropdown) return;

        const dropdownBtn = dropdown.querySelector(".dropdown-toggle");
        if (!dropdownBtn) return;

        // Set button text
        dropdownBtn.innerText = clickedItem.innerText.trim();

        // Set or remove data-model-result-value
        const resultValue = clickedItem.getAttribute("data-model-result-value");
        if (resultValue !== null) {
            dropdownBtn.setAttribute("data-model-result-value", resultValue);
        } else {
            dropdownBtn.removeAttribute("data-model-result-value");
        }
    });
});
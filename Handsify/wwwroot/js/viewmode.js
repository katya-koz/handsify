console.log("view mode");
import { initializePage } from "./navigation.js";
import { initializeCanvas, selectedStationInfo, cancelClick, stations, isClickOnStation, setSelectedStation } from "./canvas.js";
document.addEventListener("DOMContentLoaded", function () {
    //initialize
    initializePage();
    initializeCanvas();
    
    const canvas = document.getElementById("podMapCanvas");
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

function hideStationInfo() {
        document.getElementById("station-info-container").innerHTML = null;
    setSelectedStation(-1);

}


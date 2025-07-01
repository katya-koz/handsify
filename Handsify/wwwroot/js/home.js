document.addEventListener("DOMContentLoaded", function () {
    // disable floor and unit select

    var unitDropdown = document.getElementById("unit-dropdown");

    var floorDropdown = document.getElementById("floor-dropdown");
    // cycle through units, and mark non-available units

    unitDropdown.classList.add("disabled");

    floorDropdown.classList.add("disabled");
});
var usernamefield = document.getElementById("login-username-input");
var passwordfield = document.getElementById("login-password-input")

document.addEventListener("keypress", function (event) {
    if (event.key === "Enter") {
        event.preventDefault();
        onLoginButtonClick();
    }

})


function updateMessage(message) {
    var messageElement = document.getElementById("login-message");
    messageElement.innerHTML = message;
}

function onLoginButtonClick() {

    let username = usernamefield.value;
    let password = passwordfield.value;

    attemptLogin(username, password);
}

async function attemptLogin(username, password) {
    // Send POST request to AuthController's Authenticate method
    const response = await fetch(`auth/authenticate`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',  // You can still set the content-type if needed
            'username': username,                // Send username in the header
            'password': password,                // Send password in the header
        },
        credentials: 'same-origin'  // Send cookies with the request (if necessary)
    });

    if (response.ok) {
        const data = await response.json();

        sessionStorage.setItem("logged-in-user", JSON.stringify(data.user));  // Store user object as a JSON string
        window.location.href = "/Home";
    } else {
        const errorData = await response.text();  // Get the response body (which should be the error message)
        updateMessage(errorData);  // Display the error message returned by the server
    }



}
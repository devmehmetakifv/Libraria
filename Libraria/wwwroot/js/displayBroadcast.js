"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/ChatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveMessage", function (user, department, message) {
    console.log(message)
    var h3 = document.getElementById('broadcastMessage');
    if (h3 !== null) {
        h3.textContent = `Broadcast from ${user}[${department}]: ${message}`;
        setTimeout(() => {
            h3.className = 'form - control bg - light border - 0 small fadeAwayAnimation';
        }, 10000);

        setTimeout(() => {
            h3.remove();
        }, 11000);
    }
    else {
        var targetNavbar = document.getElementById('mainNav');

        var h3 = document.createElement('h3');
        h3.id = 'broadcastMessage';
        h3.className = 'form-control bg-light border-0 small fallDownAnimation';
        h3.style.position = 'relative';
        h3.style.zIndex = 500;
        h3.textContent = `Broadcast from ${user}[${department}]: ${message}`;

        targetNavbar.insertAdjacentElement('afterend', h3);

        setTimeout(() => {
            h3.className = 'form - control bg - light border - 0 small fadeAwayAnimation';
        }, 5000);

        setTimeout(() => {
            h3.remove();
        }, 6000);
    }
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});
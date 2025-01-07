﻿"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/ChatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

document.getElementById("sendButton").disabled = true;

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

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userNameInput").value;
    var department = document.getElementById("departmentInput").value;
    var message = document.getElementById("AnnouncementTitle").value;
    connection.invoke("SendMessage", user, department, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
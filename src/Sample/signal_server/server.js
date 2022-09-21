"use strict";

let WebSocketServer = require('ws').Server;
let port = 8080;

const fs = require('fs');
const https = require('https');

// Yes, TLS is required
const serverConfig = {
    key: fs.readFileSync('ssl/key.pem'),
    cert: fs.readFileSync('ssl/cert.pem'),
 };

// Create a server for the client html page
const handleRequest = function(request, response) {
    // Render the single client html file for any request the HTTP server receives
     console.log('request received: ' + request.url);
  
     if(request.url === '/') {
        response.writeHead(200, {'Content-Type': 'text/html'});
        response.end(fs.readFileSync('client/index.html'));
    } else if(request.url === '/webrtc.js') {
        response.writeHead(200, {'Content-Type': 'application/javascript'});
        response.end(fs.readFileSync('client/webrtc.js'));
    }
  };

  const httpsServer = https.createServer(serverConfig, handleRequest);
  httpsServer.listen(8081, '0.0.0.0');

//   server:httpsServer

  // 
let wsServer = new WebSocketServer({ port:port, server: httpsServer });
const ip = require('ip');
console.log('WebSocket Broadcaster Started - ' + 'wss://' + ip.address() + ':' + port);

wsServer.on('connection', function (ws) 
{
    console.log('## WebSocket Connection ##');

    ws.on('message', function (message) 
    {
        console.log('## Message Recieved ##');
        const json = JSON.parse(message.toString());
        console.log('\t' + message.toString());

        wsServer.clients.forEach(function each(client) {
            if (isSame(ws, client))
            {
                console.log('## Skipping Sender ##');
            }
            else 
            {
                client.send(message);
            }
        });
    });

});

function isSame(ws1, ws2) {
    return (ws1 === ws2);
}
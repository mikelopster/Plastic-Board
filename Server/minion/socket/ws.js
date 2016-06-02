var ws = require('ws');
var debug = require('debug')('minion:socket');
var connection = require('./connection');

var Socket = {};
var WebSocketServer = ws.Server;

function broadcaster(wss) {
  return function (self, message) {
    wss.clients.forEach(function (client) {
      if (client !== self)
        client.send(message);
    });
  };
}

Socket.attach = function (server) {
  var wss = new WebSocketServer({ server: server });
  wss.on('connection', function (ws) {
    connection(ws, broadcaster(wss));
    debug('New client connected');
  });
  debug('Socket attached');
};

module.exports = Socket;

var debug = require('debug')('minion:socket');
var packet = require('./packet');

var DataPacket = packet.DataPacket;
var MessagePacket = packet.MessagePacket;
var Command = {
  join: 0,
  spawn: 1,
  update: 2,
  destroy: 3
};

var packetIds = [];
var packetTable = {};

function resetPacket() {
  packetIds = [];
  packetTable = {};
}

function createPacket(id, packet) {
  packetIds.push(id);
  packetTable[id] = {
    spawn: packet,
    update: undefined
  };
  console.log('Create Packet', id, packet);
}

function updatePacket(id, packet) {
  packetTable[id].update = packet;
  console.log('Update Packet', id, packet);
}

function removePacket(id) {
  delete packetTable[id];
  packetIds.splice(packetIds.indexOf(id), 1);
  console.log('Remove Packet', id);
}

function onJoin(ws) {
  packetIds.forEach(function (id) {
    var message = DataPacket.message(Command.spawn, packetTable[id].spawn);
    if (message)
      ws.send(message);
  });

  packetIds.forEach(function (id) {
    var message = DataPacket.message(Command.spawn, packetTable[id].state);
    if (message)
      ws.send(message);
  });
}

function onMessage(ws, broadcast) {
  ws.on('message', function (message) {
    var dataPacket = DataPacket.value(message);
    var packet = dataPacket.packet;

    if (!DataPacket.verify(dataPacket) &&
        !MessagePacket.verify(packet))
      return;

    switch (dataPacket.command) {
    case Command.join:
      break;
    case Command.spawn:
      createPacket(packet.id, packet);
      break;
    case Command.update:
      updatePacket(packet.id, packet);
      break;
    case Command.destroy:
      removePacket(packet.id);
      break;
    }

    broadcast(ws, message);
  });
}

function connection(ws, broadcast) {
  onJoin(ws);
  onMessage(ws, broadcast);
}

module.exports = connection;

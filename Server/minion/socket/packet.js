var debug = require('debug')('minion:socket');

function isUndefined(value) {
  if (typeof value === 'undefined')
    return true;
  return false;
}

function exist(obj, prop) {
  if (isUndefined(obj[prop]))
    return false;
  return true;
}

var DataPacket = {
  verify: function (dataPacket) {
    return exist(dataPacket, 'command') &&
           exist(dataPacket, 'packet');
  },

  value: function (message) {
    var dataPacket = JSON.parse(message);
    return dataPacket;
  },

  message: function (command, packet) {
    if (isUndefined(command) || isUndefined(packet))
      return;
    var dataPacket = { command: command, packet: packet };
    var message = JSON.stringify(dataPacket);
    return message;
  }
};

var MessagePacket = {
  verify: function (messagePacket) {
    return exist(messagePacket, 'id') &&
           exist(messagePacket, 'type') &&
           exist(messagePacket, 'message');
  },

  value: function (message) {},
  message: function (id, type, message) {}
};

module.exports.DataPacket = DataPacket;
module.exports.MessagePacket = MessagePacket;

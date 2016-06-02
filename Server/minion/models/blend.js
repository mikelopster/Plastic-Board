var mongoose = require('mongoose');

var schema = mongoose.Schema({
  name: String,
  display: String,
  obj: String,
  files: [{ name: String, path: String }]
});

var Blend = mongoose.model('blend', schema);

module.exports = Blend;

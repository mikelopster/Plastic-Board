var express = require('express');
var Blend = require('../models/blend');

var router = express.Router();

router.get('/', function (req, res, next) {
  var context = {};

  Blend.find().exec()
    .then(function (docs) {
      var blends = [];
      docs.forEach(function (doc) {
        blends.push({ display: doc.display });
      });
      return blends;
    })
    .then(function (blends) {
      context.blends = blends;
      res.render('index', context);
    });
});

module.exports = router;

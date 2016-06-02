var express = require('express');
var fs = require('fs');
var path = require('path');
var Blend = require('../models/blend');

function computeDownloadPath(req) {
  return path.join(req.app.get('uploads'), req.query.name, req.query.file);
}

var router = express.Router();

router.get('/blends', function (req, res, next) {
  Blend.find().exec()
    .then(function (docs) {
      var blends = [];
      docs.forEach(function (doc) {
        blends.push({
          name: doc.name,
          display: doc.display,
          obj: doc.obj,
          files: doc.files.map(function (x) { return x.name; })
        });
      });
      return { blends: blends };
    })
    .then(function (blends) {
      res.json(blends);
    });
});

router.get('/blend', function (req, res, next) {
  fs.createReadStream(computeDownloadPath(req)).pipe(res);
});

module.exports = router;

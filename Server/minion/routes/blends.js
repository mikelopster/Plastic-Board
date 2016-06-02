var express = require('express');
var fs = require('fs');
var path = require('path');
var multer = require('multer');
var Blend = require('../models/blend');

var router = express.Router();

function computeUploadPath(req) {
  return path.join(req.app.get('uploads'), req.body.name);
}

function computeDownloadPath(req) {
  return path.join(req.app.get('uploads'), req.query.name, req.query.file);
}

var storage = multer.diskStorage({
  destination: function (req, file, cb) {
    cb(null, computeUploadPath(req));
  },
  filename: function (req, file, cb) {
    cb(null, file.originalname);
  }
});

var filter = function (req, file, cb) {
  if (req.body.name) {
    var dest = computeUploadPath(req);
    if (!fs.existsSync(dest))
      fs.mkdirSync(dest);
    req.accepted = true;
  }
  cb(null, req.accepted);
};

var upload = multer({ storage: storage, fileFilter: filter });

router.post('/upload', function (req, res, next) {
  upload.array('files')(req, res, function (err) {
    if (req.accepted) {
      var blend = new Blend({
        name: req.body.name,
        display: req.body.display,
        obj: req.body.obj,
        files: req.files.map(function (file) {
          return { name: file.originalname, path: file.path };
        })
      });
      blend.save();
      req.flash('success', 'File upload completed');
    } else {
      req.flash('error', 'File has been rejected');
    }
    res.redirect('/');
  });
});

router.get('/download', function (req, res, next) {
  res.download(computeDownloadPath(req));
});

module.exports = router;

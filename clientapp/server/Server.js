const express = require('express');
const cors = require('cors');
const fs = require('fs');
const path = require('path');

const app = express();
const port = 3001;


app.use(cors());


app.listen(port, () => {
  console.log(`File server is running at http://localhost:${port}`);
});
app.use(express.static(path.resolve(__dirname, '..\\src\\ConvertedFiles')));

app.get('/getfileDirectory', (req, res) => {
    const fileDirectory = path.resolve(__dirname, '..\\src\\ConvertedFiles');
    if (!fs.existsSync(fileDirectory)) {
      fs.mkdirSync(fileDirectory, { recursive: true });
    }
    res.json({fileDirectory});
  });

  app.delete('/deleteFile/:filename', (req, res) => {
    const filename = req.params.filename;
    const filePath = path.resolve(__dirname, `..\\src\\ConvertedFiles\\${filename}`);
  
    fs.unlink(filePath, (err) => {
      if (err) {
        console.error(`Error deleting file ${filePath}: ${err.message}`);
        res.status(500).send(`Error deleting file ${filename}`);
      } else {
        console.log(`File ${filename} deleted successfully`);
        res.status(200).send(`File ${filename} deleted successfully`);
      }
    });
  });
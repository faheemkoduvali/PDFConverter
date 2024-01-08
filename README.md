I have used Puppeteer Sharp for converting Html file to PDF.

frontend: Reacp app
Backend: .NET Core WebApi

** FrontEnd and backend connection
- I have used to to API for converting the file
    1. One is used for normal request (eg: 'https://localhost:7291/PDFConverter/PDFConverter')
    2. The other one is used when a server got restarted and the normal API request failed. (eg: 'https://localhost:7291/PDFConverter/PDFConverterOnFailure')
         - when normal request is failed, we call the PDFConverterOnFailure Api which retry for multiple times.

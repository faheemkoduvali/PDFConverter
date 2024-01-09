I have used Puppeteer Sharp for converting HTML files to PDF.

frontend: Reacp app
Backend: .NET Core WebApi

** FrontEnd and backend connection
- I have used API for converting the file
    1. An API is called along with the file to convert from frontend and the backend will send the converted file back
    2. If the backend Server got restarted after receiving an API request,
       - then upon restart the backend server will convert the file to PDF and save the converted file in a folder
       - Meanwhile, in the front end, we will check whether the converted file exists in the converted file folder
         
** Install package for frontend
    - go to the main folder
    - cd clientapp
    - npm init
    - npm install
    - npm install concurrently@8.2.2
    - npm cors@2.8.5

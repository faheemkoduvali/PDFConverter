import React, { useState,useEffect } from 'react';



const FileUpload = () => {
    const [file, setFile] = useState(null);
    const [convertedFiles, setConvertedFiles] = useState([]);
    const [loading, setLoading] = useState(false);
    const [fileInputKey, setFileInputKey] = useState(0);
    const [fileDirectoryPath, setfileDirectoryPath] = useState(null);


    // useEffect(() => {
    //     const fetchData = async () => {
    //       try {
    //         const response = await fetch('http://localhost:3001/getFileDirectory');
    //         const data = await response.json();
    //         setfileDirectoryPath(data.fileDirectory);
    //       } catch (error) {
    //         console.error('Error fetching file directory:', error);
    //       }
    //     };
    
    //     fetchData();
    //   }, []);


    const handleFile = (event) => {
        setFile(event.target.files[0]);
    };

    const convert = async (event) => {
        event.preventDefault();

        if (!file) {
            alert('Please select a file to convert.');
            return;
        }

        try {
            setLoading(true);

            const formData = new FormData();
            formData.append('htmlFile', file);
            try {
                const response = await fetch('https://localhost:7291/PDFConverter', {
                    method: 'POST',
                    body: formData,
                });

                if (response.ok) {
                    const filename = file.name.split('.')[0] + '.pdf';
                    const blob = await response.blob();
                    const blobUrl = window.URL.createObjectURL(blob);

                    setConvertedFiles((prevConvertedFiles) => [
                        ...prevConvertedFiles,
                        { name: filename, blobUrl },
                    ]);
                    // Clear the file input after successful conversion
                    setFile(null);
                    setFileInputKey((prevKey) => prevKey + 1);

                    return; // Break out of the retry loop on success
                }
                else {
                    throw new Error('Internal Server error.');
                }
            }
            catch (err) {
                const filename = file.name.split('.')[0] + '.pdf';
                const maxRetries = 5;
                let retryCount = 0;

                while (retryCount < maxRetries) {
                    try {
                        const response = await fetch(`http://localhost:3001/${filename}`);
                        if (response.ok) {
                            const blob = await response.blob();
                            const blobUrl = window.URL.createObjectURL(blob);

                            setConvertedFiles((prevConvertedFiles) => [
                                ...prevConvertedFiles,
                                { name: filename, blobUrl },
                            ]);

                            // Clear the file input after successful conversion
                            setFile(null);
                            setFileInputKey((prevKey) => prevKey + 1);

                            await deleteFile(filename);
                            return;
                        } else {
                            throw new Error('Server error.');
                        }
                    } catch (retryError) {
                        retryCount++;

                        await new Promise((resolve) => setTimeout(resolve, 3000));
                    }
                }

                throw new Error('File conversion failed.');
            }
        } catch (error) {
            console.error(error);
            alert(error.message);
        } finally {
            setLoading(false);
        }
    };

    const downloadFile = (blobUrl, filename) => {
        const link = document.createElement('a');
        link.href = blobUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    };

    const deleteFile = async (fileName) => {
        
        await fetch(`http://localhost:3001/deleteFile/${fileName}`, {
        method: 'DELETE',
        })
        .then((response) => {
            if (!response.status !== 200) {
            return 0;
            }
            return 1;
        })
    }

    return (
        <div>
            <h2>Upload your HTML file</h2>
            <form onSubmit={convert}>
                <input key={fileInputKey} type='file' name='file' onChange={handleFile} />
                <button type='submit' disabled={loading}>
                    {loading ? 'Converting...' : 'Convert'}
                </button>
            </form>

            <div>
                <h2>List of Converted Files</h2>
                <ul>
                    {convertedFiles.map((convertedFile, index) => (
                        <li key={index}>
                            {convertedFile.name}
                            <button
                                onClick={() => downloadFile(convertedFile.blobUrl, convertedFile.name)}
                            >
                                Download
                            </button>
                        </li>
                    ))}
                </ul>
            </div>
        </div>
    );
};

export default FileUpload;

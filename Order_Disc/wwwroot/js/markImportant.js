function MarkImportantFolder(folderId) {
    if (!folderId) {
        alert('Invalid folder ID.');
        return;
    }

    // Get the active section
    const activeSection = document.querySelector('.display-section.active');
    if (!activeSection) {
        console.error('No active section found.');
        return;
    }

    fetch(`/Folder/MarkImportantFolder/${folderId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ type: 'folder' }),
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to mark folder as important.');
            }
            return response.json();
        })
        .then(data => {
            console.log(data.message);

            // Refresh folder view
            const loaderFunctionName = activeSection.getAttribute('data-loader');
            if (loaderFunctionName && typeof window[loaderFunctionName] === 'function') {
                console.log(`Reloading section with loader: ${loaderFunctionName}`);
                window[loaderFunctionName](); // Refresh active section view
            } else {
                console.warn('No loader function defined for active section.');
            }
        })
        .catch(error => {
            console.error('Error marking folder as important:', error.message);
        });
}

function MarkImportantSharedFolder(folderShareId) {
    console.log(`Attempting to mark shared folder as important. ID: ${folderShareId}`);

    if (!folderShareId) {
        alert('Invalid shared folder ID.');
        return;
    }

    fetch(`/Folder/MarkImportantSharedFolder/${folderShareId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
    })
        .then(response => {
            console.log(`Response status: ${response.status}`);
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(`Error ${response.status}: ${text}`);
                });
            }
            return response.json();
        })
        .then(data => {
            console.log('Server response:', data.message);

            const activeSection = document.querySelector('.display-section.active');
            if (activeSection) {
                const loaderFunctionName = activeSection.getAttribute('data-loader');
                if (loaderFunctionName && typeof window[loaderFunctionName] === 'function') {
                    console.log(`Reloading section with loader: ${loaderFunctionName}`);
                    window[loaderFunctionName]();
                } else {
                    console.warn('No loader function defined for active section.');
                }
            }
        })
        .catch(error => {
            console.error('Error marking shared folder as important:', error.message);
            alert(`An error occurred: ${error.message}`);
        });
}

function MarkImportantFile(fileId) {
    if (!fileId) {
        alert('Invalid file ID.');
        return;
    }

    const activeSection = document.querySelector('.display-section.active');
    if (!activeSection) {
        console.error('No active section found.');
        return;
    }

    const currentFolderId = activeSection.getAttribute('data-current-folder-id');

    fetch(`/Important/MarkImportantFile/${fileId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ type: 'file' }),
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to mark file as important.');
            }
            return response.json();
        })
        .then(data => {
            console.log(data.message);

            if (currentFolderId) {
                console.log(`Reloading folder contents for folder ID: ${currentFolderId}`);
                navigateToFolder(currentFolderId);
            } else {
                console.warn('No current folder ID available.');
            }
        })
        .catch(error => {
            console.error('Error marking file as important:', error.message);
        });
}

function MarkImportantSharedFile(fileShareId) {
    console.log(`Attempting to mark shared file as important. ID: ${fileShareId}`);

    if (!fileShareId) {
        alert('Invalid shared file ID.');
        return;
    }

    fetch(`/Important/MarkImportantSharedFile/${fileShareId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
    })
        .then(response => {
            console.log(`Response status: ${response.status}`);
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(`Error ${response.status}: ${text}`);
                });
            }
            return response.json();
        })
        .then(data => {
            console.log('Server response:', data.message);

            const fileRow = document.querySelector(`#shared-folder-contents [data-file-id="${fileShareId}"]`);
            if (fileRow) {
                console.log(`Reloading item with ID: ${fileShareId}`);
                fileRow.classList.add('important');
            } else {
                console.warn(`No DOM element found with data-file-id=${fileShareId}`);
            }
        })
        .catch(error => {
            console.error('Error marking shared file as important:', error.message);
            alert(`An error occurred: ${error.message}`);
        });
}

function unmarkImportant(itemId, itemType) {
    if (!itemId || !itemType) {
        alert('Invalid item ID or type.');
        return;
    }

    const endpoint = `/Folder/UnmarkImportant/${itemId}?type=${itemType}`;
    console.log(`Unmarking item: ID = ${itemId}, Type = ${itemType}`);

    fetch(endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text);
                });
            }
            return response.json();
        })
        .then(data => {
            console.log(data.message);
            loadImportantFolders();
        })
        .catch(error => {
            console.error(`Error unmarking ${itemType}:`, error.message);
        });
}

function getItemTypeName(itemType) {
    switch (itemType) {
        case 'folder': return 'Folder';
        case 'file': return 'File';
        case 'shared-folder': return 'Shared Folder';
        case 'shared-file': return 'Shared File';
        default: return 'Item';
    }
}

function loadTrashContents() {
    const trashSection = document.getElementById('trash-section');
    if (!trashSection) {
        console.error('Trash section not found in DOM.');
        console.log('Current DOM:', document.body.innerHTML);
        return;
    }

    console.log('Trash section found. Loading trash contents...');

    fetch('/Trash/GetTrashContents', { method: 'GET' })
        .then(response => {
            console.log('Server response status (TrashContents):', response.status);
            if (!response.ok) {
                throw new Error('Failed to load trash contents.');
            }
            return response.text();
        })
        .then(html => {
            console.log('HTML received for trash contents:', html);
            trashSection.innerHTML = html;
            console.log('Trash contents loaded successfully.');
        })
        .catch(error => {
            console.error('Error loading trash contents:', error.message);
        });
}

function moveToTrash(itemId, itemType) {
    if (!itemId || !itemType) {
        alert('Invalid item ID or type.');
        return;
    }

    const activeSection = document.querySelector('.display-section.active');
    const currentFolderId = activeSection?.getAttribute('data-current-folder-id');

    if (!currentFolderId) {
        console.error('Current folder ID is missing.');
        return;
    }

    fetch(`/Trash/MoveToTrash/${itemId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ type: itemType }),
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to move item to Trash.');
            }
            return response.json();
        })
        .then(data => {
            console.log(data.message);
            console.log(`Reloading folder contents for folder ID: ${currentFolderId}`);
            navigateToFolder(currentFolderId);
        })
        .catch(error => {
            console.error('Error moving item to Trash:', error.message);
            alert('An error occurred while moving the item to Trash.');
        });
}

window.MoveFolderToTrash = function (folderId) {
    console.log('Attempting to move folder with ID:', folderId);

    if (!folderId) {
        alert('Invalid folder ID.');
        return;
    }

    fetch(`/Folder/MoveFolderToTrash/${folderId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
    })
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text); });
            }
            return response.json();
        })
        .then(data => {
            console.log('Server response:', data);
            const activeSection = document.querySelector('.display-section.active');
            if (!activeSection) return;

            const loaderFunctionName = activeSection.getAttribute('data-loader');
            if (loaderFunctionName && typeof window[loaderFunctionName] === 'function') {
                window[loaderFunctionName]();
            }
        })
        .catch(error => {
            console.error('Error:', error.message);
            alert('An error occurred.');
        });
};

function moveSharedFolderToTrash(folderShareId) {
    console.log(`Attempting to move shared folder with ID: ${folderShareId}`);

    if (!folderShareId) {
        alert('Invalid shared folder ID.');
        return;
    }

    fetch(`/Trash/MoveSharedFolderToTrash/${folderShareId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
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
            console.log('Server response:', data);
            const activeSection = document.querySelector('.display-section.active');
            if (!activeSection) {
                console.error('No active section found.');
                return;
            }

            const loaderFunctionName = activeSection.getAttribute('data-loader');
            if (loaderFunctionName && typeof window[loaderFunctionName] === 'function') {
                console.log(`Reloading section with loader: ${loaderFunctionName}`);
                window[loaderFunctionName]();
            } else {
                console.warn('No loader function defined for active section.');
            }
        })
        .catch(error => {
            console.error('Error moving shared folder to trash:', error.message);
            alert(`An error occurred: ${error.message}`);
        });
}

function restoreItem(itemId, itemType) {
    console.log(`Restoring item: ID = ${itemId}, Type = ${itemType}`);
    if (!itemId || !itemType || itemId === 0) {
        alert('Invalid item ID or type.');
        return;
    }

    const endpoint = `/Trash/Restore/${itemId}?type=${itemType}`;

    fetch(endpoint, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
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
            loadTrashContents();
        })
        .catch(error => {
            console.error(`Error restoring ${itemType}:`, error.message);
            alert(`An error occurred while restoring the ${getItemTypeName(itemType)}.`);
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

function deleteAllTrash() {
    if (!confirm('Are you sure you want to delete all items from trash? This action cannot be undone.')) {
        return;
    }

    fetch('/Trash/DeleteAll', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
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
            loadTrashContents();
        })
        .catch(error => {
            console.error('Error deleting all trash:', error.message);
            alert('An error occurred while deleting all items in trash.');
        });
}

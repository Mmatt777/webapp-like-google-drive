function downloadItem(type, id) {
    let downloadUrl;
    if (type === 'file') {
        downloadUrl = `/Download/file/${id}`;
    } else if (type === 'folder') {
        downloadUrl = `/Download/folder/${id}`;
    } else {
        console.error('Unknown item type for download:', type);
        return;
    }

    fetch(downloadUrl, { method: 'GET' })
        .then(response => {
            if (!response.ok) {
                alert(`Download error: ${response.statusText}`);
                return;
            }

            const link = document.createElement('a');
            link.href = downloadUrl;
            link.download = '';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        })
        .catch(error => {
            console.error('Error during download:', error);
            alert('A problem occurred while downloading.');
        });
}

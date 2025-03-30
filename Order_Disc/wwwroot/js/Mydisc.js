function openNewFolderModal() {
    const modalContainer = document.getElementById("newFolderSection");
    modalContainer.classList.add("show");
}

function closeNewFolderModal() {
    const modalContainer = document.getElementById("newFolderSection");
    modalContainer.classList.remove("show");
}

document.addEventListener('DOMContentLoaded', function () {
    const defaultSectionId = 'documents-type';
    showSection(defaultSectionId);
});

function showSection(sectionId) {
    const sections = document.querySelectorAll('.display-section');
    if (!sections.length) {
        console.error('No sections found.');
        return;
    }

    sections.forEach(section => {
        section.classList.remove('active');
        section.style.display = 'none';

        const foldersContainer = section.querySelector('[id$="-folders"]');
        const folderContents = section.querySelector('[id$="-folder-contents"]');
        if (foldersContainer) foldersContainer.style.display = 'block';
        if (folderContents) folderContents.style.display = 'none';
    });

    const selectedSection = document.getElementById(sectionId);
    if (!selectedSection) {
        console.error(`Section with ID "${sectionId}" not found.`);
        return;
    }

    selectedSection.classList.add('active');
    selectedSection.style.display = 'block';
    console.log(`Section "${sectionId}" activated.`);

    const loaderFunctionName = selectedSection.getAttribute('data-loader');
    if (loaderFunctionName && typeof window[loaderFunctionName] === 'function') {
        try {
            console.log(`Calling loader function: ${loaderFunctionName}`);
            window[loaderFunctionName]();
        } catch (error) {
            console.error(`Error while calling loader function: ${loaderFunctionName}`, error);
        }
    } else if (loaderFunctionName) {
        console.warn(`Loader function "${loaderFunctionName}" is not defined.`);
    }
}

function sortTable(containerSelector, columnIndex) {
    console.log(`Sorting table: ${containerSelector}, column: ${columnIndex}`);

    const table = document.querySelector(`${containerSelector} tbody`);
    if (!table) {
        console.error(`Table not found: ${containerSelector}`);
        return;
    }

    const rows = Array.from(table.rows);
    if (rows.length === 0) {
        console.warn(`No rows to sort in table: ${containerSelector}`);
        return;
    }

    if (!window.sortOrder) {
        window.sortOrder = {};
    }

    window.sortOrder[columnIndex] = !window.sortOrder[columnIndex];
    const ascending = window.sortOrder[columnIndex];

    rows.sort((rowA, rowB) => {
        const cellA = rowA.cells[columnIndex]?.innerText.trim() || "";
        const cellB = rowB.cells[columnIndex]?.innerText.trim() || "";

        const numA = parseFloat(cellA.replace(",", "."));
        const numB = parseFloat(cellB.replace(",", "."));

        if (!isNaN(numA) && !isNaN(numB)) {
            return ascending ? numA - numB : numB - numA;
        }

        if (Date.parse(cellA) && Date.parse(cellB)) {
            return ascending ? new Date(cellA) - new Date(cellB) : new Date(cellB) - new Date(cellA);
        }

        return ascending ? cellA.localeCompare(cellB) : cellB.localeCompare(cellA);
    });

    table.innerHTML = "";
    rows.forEach(row => table.appendChild(row));

    console.log(`Table ${containerSelector} sorted by column ${columnIndex}.`);
}

function attachSortEvents(containerSelector) {
    const table = document.querySelector(`${containerSelector} .styled-table`);
    if (!table) return;

    const headers = table.querySelectorAll("th.sortable");
    if (!headers.length) return;

    headers.forEach((header, index) => {
        header.onclick = () => sortTable(containerSelector, index);
    });
}

function loadFolders(endpoint, containerId, sectionId) {
    const section = document.getElementById(sectionId);
    if (!section) return;

    const foldersContainer = section.querySelector(containerId);
    if (!foldersContainer) return;

    fetch(endpoint, { method: 'GET' })
        .then(response => response.text())
        .then(html => {
            foldersContainer.innerHTML = html;
            attachSortEvents(containerId);
        })
        .catch(error => console.error(`Error loading ${containerId}:`, error.message));
}

function loadUserFolders() {
    loadFolders('/Folder/UserFolders', '#user-folders', 'documents-section');
}

function loadImportantFolders() {
    loadFolders('/Folder/AllImportantItems', '#important-folders', 'important-section');
}

function loadSharedFolders() {
    loadFolders('/Folder/SharedFolders', '#shared-folders', 'shared-section');
}

function navigateToFolder(folderId) {
    console.log(`Navigating to folder with ID: ${folderId}`);

    const activeSection = document.querySelector('.display-section.active');
    if (!activeSection) return;

    const foldersContainer = activeSection.querySelector('[id$="-folders"]');
    if (foldersContainer) foldersContainer.style.display = 'none';

    const folderContents = activeSection.querySelector('[id$="-folder-contents"]');
    if (!folderContents) return;

    fetch(`/Folder/Details/${folderId}`, { method: 'GET' })
        .then(response => response.text())
        .then(html => {
            folderContents.innerHTML = html;
            folderContents.style.display = 'block';
            attachSortEvents('[id$="-folder-contents"]');

            activeSection.setAttribute('data-current-folder-id', folderId);
            const uploadArea = document.getElementById('uploadArea');
            if (uploadArea) uploadArea.setAttribute('data-current-folder-id', folderId);
        })
        .catch(error => console.error('Error loading folder details:', error.message));
}
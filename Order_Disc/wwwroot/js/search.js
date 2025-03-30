let isSearchSectionInitialized = false;

function loadSearchSection() {
    console.log('Loading search section content...');

    if (isSearchSectionInitialized) {
        console.log('The search section has already been initialized.');
        return;
    }

    const searchInput = document.getElementById('searchInput');
    const searchSection = document.getElementById('search-section');
    const userFolders = document.getElementById('user-folders');

    if (!searchInput || !searchSection || !userFolders) {
        console.error('Required elements not found in the DOM.');
        return;
    }

    searchInput.addEventListener('focus', () => searchSection.style.display = 'block');

    searchInput.addEventListener('input', () => {
        const query = searchInput.value.trim();
        if (!query) {
            userFolders.innerHTML = '';
            return;
        }

        fetch(`/Search/GetSearchResults?query=${encodeURIComponent(query)}`)
            .then(response => response.ok ? response.text() : Promise.reject('Error fetching search results'))
            .then(html => userFolders.innerHTML = html)
            .catch(error => console.error('Error:', error));
    });

    isSearchSectionInitialized = true;
    console.log('The search section has been initialized.');
}

function openShareFolderModal(folderId) {
    document.getElementById("folderIdToShare").value = folderId;
    const modal = document.getElementById("ShareModalSection");
    modal.style.display = "flex";
}

function closeShareFolderModal() {
    const modal = document.getElementById("ShareModalSection");
    modal.style.display = "none";
    document.getElementById("folderIdToShare").value = "";
}

async function shareFolder(event) {
    event.preventDefault();
    const folderId = document.getElementById("folderIdToShare").value;
    const email = document.getElementById("emailToShare").value;

    try {
        let response = await fetch(`/Folder/ShareFolder`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ folderId, email }),
        });

        if (response.ok) {
            alert("Folder has been shared successfully!");
        } else if (response.status === 409) {
            const result = await response.json();
            const userConfirm = confirm(`${result.message} Do you want to overwrite it?`);

            if (userConfirm) {
                response = await fetch(`/Folder/ShareFolder?overwrite=true`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({ folderId, email }),
                });

                if (response.ok) {
                    alert("The folder has been overwritten and shared!");
                } else {
                    const error = await response.text();
                    alert(`Error while overwriting: ${error}`);
                }
            } else {
                alert("Folder sharing has been canceled.");
            }
        } else {
            const error = await response.text();
            alert(`Error: ${error}`);
        }
    } catch (error) {
        console.error("An error occurred:", error);
        alert("An error occurred while sharing the folder.");
    }

    closeShareFolderModal();
}

async function navigateToSharedFolder(fileShareId) {
    try {
        console.log(`Navigating to shared folder with ID: ${fileShareId}`);

        const folderContents = document.getElementById("shared-folder-contents");
        const sharedFolders = document.getElementById("shared-folders");

        const response = await fetch(`/Folder/SharedFiles?folderId=${fileShareId}`);
        console.log("Response status:", response.status);

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || "Failed to load shared files.");
        }

        const html = await response.text();
        console.log("Received HTML content:", html);

        folderContents.innerHTML = html;
        attachSortEvents('[id$="-folder-contents"]');
        sharedFolders.style.display = "none";
        folderContents.style.display = "block";
    } catch (error) {
        console.error("Error loading shared files:", error.message);
        alert("Failed to load shared files.");
    }
}

async function navigateToImportantSharedFolder(folderId) {
    try {
        console.log('Navigating to important shared folder with ID:', folderId);

        const folderContents = document.getElementById("important-folder-contents");
        const importantFolders = document.getElementById("important-folders");

        if (!folderContents) {
            console.error("No important folder contents container found in the DOM.");
            return;
        }

        if (!importantFolders) {
            console.error("No important folders container found in the DOM.");
            return;
        }

        console.log("Important folder contents container found:", folderContents);

        const response = await fetch(`/Folder/SharedFiles?folderId=${folderId}`);
        if (!response.ok) {
            throw new Error("Failed to load shared files.");
        }

        const html = await response.text();
        console.log("Received HTML:", html);

        folderContents.innerHTML = html;
        attachSortEvents('[id$="-folder-contents"]');
        importantFolders.style.display = "none";
        folderContents.style.display = "block";

        console.log("Important shared folder loaded successfully.");
    } catch (error) {
        console.error("Error loading important shared folder:", error.message);
        alert("Failed to load important shared folders. Please try again.");
    }
}

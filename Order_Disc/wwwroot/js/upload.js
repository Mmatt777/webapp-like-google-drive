document.addEventListener("DOMContentLoaded", () => {
    console.log("DOMContentLoaded event fired.");

    document.addEventListener("click", (event) => {
        if (event.target && event.target.id === "uploadButton") {
            console.log("Upload button clicked.");
            const fileInput = document.getElementById("fileInput");
            if (fileInput) {
                fileInput.click();
            } else {
                console.error("File input not found.");
            }
        }
    });

    document.addEventListener("change", async (event) => {
        if (event.target && event.target.id === "fileInput") {
            console.log("File input changed.");
            const files = event.target.files;

            if (files.length === 0) {
                alert("No files selected.");
                return;
            }

            const uploadArea = document.getElementById("uploadArea");
            const folderId = uploadArea.getAttribute("data-current-folder-id");
            if (!folderId) {
                console.error("No folder ID found.");
                return;
            }

            const formData = new FormData();
            for (let i = 0; i < files.length; i++) {
                formData.append("files", files[i]);
            }

            console.log("FormData to be sent:", formData);

            try {
                const response = await fetch(`/Folder/UploadFiles/${folderId}`, {
                    method: "POST",
                    body: formData,
                });

                console.log("Response status:", response.status);

                if (!response.ok) {
                    const errorText = await response.text();
                    console.error("Error response text:", errorText);
                    throw new Error(`Failed to upload files: ${errorText}`);
                }

                const result = await response.json();
                console.log("Server response:", result);

                console.log("Refreshing folder view...");
                navigateToFolder(folderId); 
            } catch (error) {
                console.error("Error uploading files:", error.message);
                alert("An error occurred while uploading files.");
            }
        }
    });
});

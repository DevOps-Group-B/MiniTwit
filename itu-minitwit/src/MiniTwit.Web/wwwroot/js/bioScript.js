function enableEditing() {
    const bioP = document.getElementById('author-bio');
    const editButton = document.getElementById('edit-button');
    const editInput = document.getElementById('editInput');
    const form = document.getElementsByClassName('bio-form')[0];
    const cancelButton = document.getElementById('cancel-button');
    
    // Hide the text and the edit button
    bioP.style.display = 'none';
    editButton.style.display = 'none';
    form.style.display = 'flex';
    cancelButton.style.display = 'flex';

    // Focus on the input field
    editInput.focus();

    // Place the cursor at the end of the text
    const length = editInput.value.length;
    editInput.setSelectionRange(length, length);
}

function disableEditing(bio){
    const bioP = document.getElementById('author-bio');
    const editButton = document.getElementById('edit-button');
    const form = document.getElementsByClassName('bio-form')[0];
    const cancelButton = document.getElementById('cancel-button');
    const editInput = document.getElementById('editInput');

    editInput.value = bio;
    cancelButton.style.display = 'none';
    form.style.display = 'none';
    bioP.style.display = 'flex';
    editButton.style.display = 'flex';
}
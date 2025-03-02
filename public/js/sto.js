document.getElementById('addModelForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    try {
        const response = await fetch(this.action, {
            method: 'POST',
            body: new FormData(this),
            headers: {
                'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').content,
                'Accept': 'application/json'
            }
        });

        const data = await response.json();

        if (response.ok) {
            // Tambahkan model baru ke tabel
            const tableBody = document.getElementById('stoTableBody');
            const newRow = document.createElement('tr');
            const model = data.model.name;
            
            newRow.innerHTML = `
                <td>${model}</td>
                <td><input type="text" name="machining_r_${model}" class="form-control small-input"></td>
                <td><input type="text" name="machining_l_${model}" class="form-control small-input"></td>
                <td><input type="text" name="predrill_r_${model}" class="form-control small-input"></td>
                <td><input type="text" name="predrill_l_${model}" class="form-control small-input"></td>
                <td><input type="text" name="repair_r_${model}" class="form-control small-input"></td>
                <td><input type="text" name="repair_l_${model}" class="form-control small-input"></td>
                <td><input type="text" name="wagon_r_${model}" class="form-control small-input"></td>
                <td><input type="text" name="wagon_l_${model}" class="form-control small-input"></td>
                <td><input type="text" name="otm_r_${model}" class="form-control small-input"></td>
                <td><input type="text" name="otm_l_${model}" class="form-control small-input"></td>
                <td><input type="text" name="rib_r_${model}" class="form-control small-input"></td>
                <td><input type="text" name="rib_l_${model}" class="form-control small-input"></td>
            `;
            
            tableBody.appendChild(newRow);
            
            // Tutup modal
            $('#addModelModal').modal('hide');
            
            // Reset form
            this.reset();
            
            // Tampilkan pesan sukses
            alert('Model baru berhasil ditambahkan');
        } else {
            alert(data.message || 'Terjadi kesalahan saat menambah model');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Terjadi kesalahan saat menambah model');
    }
});
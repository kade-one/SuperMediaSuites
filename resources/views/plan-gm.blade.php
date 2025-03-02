@extends('layouts.app')

@section('content')
<button id="addButton">Tambah</button>
<div id="dataTable" style="display:none;">
  <form id="dataForm">
    <table>
      <tr>
        <td><input type="text" name="field1" placeholder="Field 1"></td>
        <td><input type="text" name="field2" placeholder="Field 2"></td>
        <!-- Tambahkan field lainnya sesuai kebutuhan -->
      </tr>
    </table>
    <button type="submit">Submit</button>
  </form>
</div>

<script>
  document.getElementById('addButton').addEventListener('click', function() {
    document.getElementById('dataTable').style.display = 'block';
  });

  document.getElementById('dataForm').addEventListener('submit', function(event) {
    event.preventDefault();
    // Tambahkan logika untuk mengirim data ke backend
  });
</script>
@endsection
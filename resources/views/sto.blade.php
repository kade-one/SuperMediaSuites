@extends('layouts.app')

@section('content')
<h1>STO PORTAL</h1>

<!-- Formulir pengisian data langsung ditampilkan -->
<div id="dataFormContainer">
    <form id="dataForm" method="POST" action="/sto">
        @csrf
        <div class="form-container">
            <div class="form-group">
                <label for="date">Hari/Tanggal:</label>
                <input type="date" id="date" name="date" class="form-controls" required>
            </div>
            <div class="form-group">
                <label for="operator">Operator:</label>
                <input type="text" id="operator" name="operator" class="form-controls" required>
            </div>
            <div class="form-group">
                <label for="casting">Casting:</label>
                <select id="casting" name="casting" class="form-controls" required>
                    <option value="Casting 1">Casting 1</option>
                    <option value="Casting 2">Casting 2</option>
                </select>
            </div>
            <div class="form-group">
                <label for="shift">Shift:</label>
                <select id="shift" name="shift" class="form-controls" required>
                    <option value="1">Shift 1</option>
                    <option value="2">Shift 2</option>
                    <option value="3">Shift 3</option>
                </select>
            </div>
            <div class="form-group">
                <label for="jam_sto">Jam STO:</label>
                <input type="text" id="jam_sto" name="jam_sto" class="form-controls" required>
            </div>
            <div class="form-group">
                <label for="fm">FM:</label>
                <input type="text" id="fm" name="fm" class="form-controls" required>
            </div>
        </div>
        <a href="{{ route('models.index') }}" class="btn btn-primary mb-3">
            <i class="fas fa-plus"></i> Kelola Model
        </a>
        
        <!-- Table wrapper with horizontal scroll -->
        <div style="width: 100%; overflow-x: auto; margin-bottom: 20px;">
            <table class="table table-bordered" style="min-width: 1200px;">
                <thead>
                    <tr>
                        <th>Model</th>
                        <th colspan="2">Siap Machining</th>
                        <th colspan="2">Siap Predrill</th>
                        <th colspan="2">Siap Repair</th>
                        <th colspan="2">Wagon Gravity</th>
                        <th colspan="2">Line OTM</th>
                        <th colspan="2">Siap Rib</th>
                    </tr>
                    <tr>
                        <th><input type="text" id="modelSearch" class="form-control" placeholder="Cari Model"></th>
                        <th>R</th>
                        <th>L</th>
                        <th>R</th>
                        <th>L</th>
                        <th>R</th>
                        <th>L</th>
                        <th>R</th>
                        <th>L</th>
                        <th>R</th>
                        <th>L</th>
                        <th>R</th>
                        <th>L</th>
                    </tr>
                </thead>
                <tbody id="stoTableBody">
                    @foreach($models as $model)
                    <tr>
                        <td>{{ $model }}</td>
                        <td><input type="text" name="machining_r_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="machining_l_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="predrill_r_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="predrill_l_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="repair_r_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="repair_l_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="wagon_r_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="wagon_l_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="otm_r_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="otm_l_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="rib_r_{{ $model }}" class="form-control small-input"></td>
                        <td><input type="text" name="rib_l_{{ $model }}" class="form-control small-input"></td>
                    </tr>
                    @endforeach
                </tbody>
            </table>
        </div>
        <button type="submit" class="btn btn-success">Simpan Data</button>
    </form>
</div>

    <style>

    .btn {
        background-color: #666;
        padding-top: 15px;
        padding-bottom: 15px;
        padding-left: 30px;
        padding-right: 30px;
        margin: 15px;
    }
    /* .btn-success {
        margin-top: 10px;
        margin-bottom: 5px;
        background-color: #3b82f6;
        border: none;
        padding: 0.5rem 1.5rem;
        font-weight: 500;
        border-radius: 6px;
        transition: all 0.2s ease;
    } */
    .btn-success:hover {
        background-color: #2563eb;
        transform: translateY(-1px);
        }

    /* .btn-primary {
        margin-top: 10px;
        margin-bottom: 5px;
        background-color: #3b82f6;
        border: none;
        padding: 0.5rem 1.5rem;
        font-weight: 500;
        border-radius: 6px;
        transition: all 0.2s ease;
        } */

    .btn-primary:hover {
        background-color: #2563eb;
        transform: translateY(-1px);
        }
    .small-input {
        width: 60px !important;
        padding: 4px !important;
        }

    .form-container {
        margin-bottom: 20px;
        }

    .form-group {
        margin-bottom: 15px;
        }
            

</style>

<script>
document.getElementById('modelSearch').addEventListener('keyup', function() {
    var input, filter, table, tr, td, i, txtValue;
    input = document.getElementById('modelSearch');
    filter = input.value.toUpperCase();
    table = document.getElementById('stoTableBody');
    tr = table.getElementsByTagName('tr');

    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName('td')[0];
        if (td) {
            txtValue = td.textContent || td.innerText;
            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = '';
            } else {
                tr[i].style.display = 'none';
            }
        }       
    }
});

document.getElementById('dataForm').addEventListener('submit', function(event) {
    event.preventDefault();
    alert('Data STO berhasil disimpan');
    this.submit();
});
</script>
@endsection
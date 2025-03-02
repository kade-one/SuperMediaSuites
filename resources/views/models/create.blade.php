@extends('layouts.app')

@section('content')
<div class="container">
    <h1>Manajemen Model</h1>
    
    <!-- Form untuk menambah model baru -->
    <div class="container mb-4">
        <div class="container-body">
            <form method="POST" action="{{ route('model.store') }}">
                @csrf
                <div class="form-control">
                    <label for="modelName" class="nama-model">Tambah Model:</label>
                    <input type="text" class="form-control" id="modelName" name="name" required>
                </div>
                <button type="submit" class="btn btn-primary mt-3">Simpan Model</button>
            </form>
        </div>
    </div>

    <!-- Tabel daftar model yang ada -->
    <div class="container">

        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-bordered">
                    <thead>
                        <tr>
                            <th>Nama Model</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach($models as $model)
                        <tr>
                            <td>{{ $model->name }}</td>
                            <td>{{ $model->is_active ? 'Aktif' : 'Tidak Aktif' }}</td>
                        </tr>
                        @endforeach
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    @if(session('success'))
        <div class="alert alert-success mt-3">
            {{ session('success') }}
        </div>
    @endif

    @if(session('error'))
        <div class="alert alert-danger mt-3">
            {{ session('error') }}
        </div>
    @endif
</div>

<style>
    .container {
        margin: 15px;
    }

    .form-control {
        padding: 15px;
        margin-bottom: 15px;
        font-size: large;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }

    .nama-model {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }

    .btn {
        padding: 15px;
    }
</style>
@endsection

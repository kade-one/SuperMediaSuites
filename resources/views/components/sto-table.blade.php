<div class="sto-data mb-4">
    <div class="sto-info">
        <p><strong>Tanggal:</strong> {{ $sto->date }}</p>
        <p><strong>Operator:</strong> {{ $sto->operator }}</p>
        <p><strong>Shift:</strong> {{ $sto->shift }}</p>
        <p><strong>Casting:</strong> {{ $sto->casting }}</p>
        <p><strong>Jam STO:</strong> {{ $sto->jam_sto }}</p>
        <p><strong>FM:</strong> {{ $sto->fm }}</p>
    </div>
</div>
<table class="table table-bordered">
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
            <th></th>
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
    <tbody>
    @foreach($models as $model)
        <tr>
            <td>{{ $model }}</td>
            <td>{{ $sto->data["machining_r_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["machining_l_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["predrill_r_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["predrill_l_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["repair_r_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["repair_l_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["wagon_r_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["wagon_l_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["otm_r_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["otm_l_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["rib_r_{$model}"] ?? '' }}</td>
            <td>{{ $sto->data["rib_l_{$model}"] ?? '' }}</td>
        </tr>
        @endforeach
    </tbody>
</table>


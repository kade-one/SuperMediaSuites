<?php

namespace App\Exports;

use App\Models\Sto; // Ganti dengan model Anda yang sebenarnya
use Maatwebsite\Excel\Concerns\FromCollection;

class CumulativeExport implements FromCollection
{
    public function collection()
    {
        // Ganti dengan query yang sesuai untuk mendapatkan data kumulatif
        return Sto::all(); // Sesuaikan sesuai kebutuhan
    }
}

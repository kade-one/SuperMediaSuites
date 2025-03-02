<?php

namespace App\Exports;

use App\Models\Sto; // Ganti dengan model Anda yang sebenarnya
use Maatwebsite\Excel\Concerns\FromCollection;

class Casting2Export implements FromCollection
{
    public function collection()
    {
        // Ganti dengan query yang sesuai untuk mendapatkan data Casting 2
        return Sto::where('casting', '2')->get();
    }
}

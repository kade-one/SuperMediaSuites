<?php

namespace App\Exports;

use App\Models\Sto;
use Maatwebsite\Excel\Concerns\FromCollection;

class Casting1Export implements FromCollection
{
    public function collection()
    {
        // Ganti dengan query yang sesuai untuk mendapatkan data Casting 1
        return Sto::where('casting', '1')->get();
    }
}
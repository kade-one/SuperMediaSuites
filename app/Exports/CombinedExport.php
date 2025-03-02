<?php

namespace App\Exports;

use Maatwebsite\Excel\Concerns\WithMultipleSheets;

class CombinedExport implements WithMultipleSheets
{
    protected $date;
    protected $shift;

    public function __construct($date, $shift)
    {
        $this->date = $date;
        $this->shift = $shift;
    }

    public function sheets(): array
    {
        return [
            new Casting1Export(),
            new Casting2Export(),
            new CumulativeExport(),
        ];
    }
}
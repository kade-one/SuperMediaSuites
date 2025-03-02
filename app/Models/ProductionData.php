<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;

class ProductionData extends Model
{
    use HasFactory;

    protected $fillable = [
        'machine_id',
        'date',
        'shift',
        'output_act',
        'tc',
        'st',
        'subtotal'
    ];

    protected $casts = [
        'date' => 'date',
        'output_act' => 'integer',
        'tc' => 'integer',
        'st' => 'integer',
        'subtotal' => 'integer'
    ];

    public function machine()
    {
        return $this->belongsTo(Machine::class);
    }
}
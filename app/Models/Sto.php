<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Sto extends Model
{
    protected $fillable = ['date', 'operator', 'casting', 'shift', 'jam_sto', 'fm', 'data'];
}
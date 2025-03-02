<div class="sidebar">
    <div class="sidebar-header">
        <img src="{{ asset('images/kyb.png') }}" alt="Logo" class="logo">
        <div class="profile">
            <img src="{{ asset('images/user.png') }}" alt="Profile" class="profile-img">
            @if (Auth::check())
                <p class="profile-name">{{ Auth::user()->name }}</p>
                <!-- <p class="profile-email">{{ Auth::user()->email }}</p> -->
            @else
                <p class="profile-name">Guest</p>
            @endif
        </div>
    </div>
    <ul>
        <li><a href="/dashboard">DASHBOARD</a></li>
        <li><a href="/sto-display">DISPLAY</a></li>
        <li><a href="/sto">STO</a></li>
        <li><a href="/plan-gm">PLAN GM</a></li>
        <li><a href="/lhp">LHP</a></li>
        <li><a href="/mtn-core">MTN CORE</a></li>
    </ul>
    <form method="POST" action="{{ route('logout') }}">
        @csrf
        <button type="submit" class="btn btn-danger">Logout</button>
    </form>
</div>
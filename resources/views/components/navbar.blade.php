<!-- HTML -->
<div class="navbar">
    <div class="navbar-header">
        <img src="{{ asset('images/kyb.png') }}" alt="Logo" class="logo">
        <div class="menu-toggle">
            <div class="menu-icon">
                <span></span>
                <span></span>
                <span></span>
            </div>
        </div>
        <ul class="nav-links">
            <li><a href="/dashboard">DASHBOARD</a></li>
            <li><a href="/sto">STO</a></li>
            <li><a href="/sto-display">STO DISPLAY</a></li>
            <!-- <li><a href="/plan-gm">PLAN GM</a></li> -->
            <li><a href="/lhp">LHP</a></li>
            <li><a href="{{ route('lhp.display') }}">LHP DISPLAY</a></li>
            <!-- Mobile-only user info and logout -->
            <li class="mobile-user-section">
                <div class="mobile-user-info">
                    <img src="{{ asset('images/user.png') }}" alt="Profile" class="mobile-profile-img">
                    @if (Auth::check())
                        <span class="mobile-user-name">{{ Auth::user()->name }}</span>
                    @else
                        <span class="mobile-user-name">Guest</span>
                    @endif
                </div>
                <form method="POST" action="{{ route('logout') }}" class="mobile-logout-form">
                    @csrf
                    <button type="submit" class="btn btn-danger mobile-logout-btn">Logout</button>
                </form>
            </li>
        </ul>
    </div>
    <div class="profile-dropdown">
        <div class="profile">
            <img src="{{ asset('images/user.png') }}" alt="Profile" class="profile-img">
            @if (Auth::check())
                <p class="profile-name">{{ Auth::user()->name }}</p>
            @else
                <p class="profile-name">Guest</p>
            @endif
        </div>
        <div class="profile-dropdown-content">
            <form method="POST" action="{{ route('logout') }}">
                @csrf
                <button type="submit" class="btn btn-danger">Logout</button>
            </form>
        </div>
    </div>
</div>

<!-- Complete CSS -->
<style>
:root {
    --primary-color: #2c3e50;
    --accent-color: #3498db;
    --hover-color: #e74c3c;
    --text-color: #2c3e50;
    --transition-speed: 0.3s;
    --background-color: rgba(255, 255, 255, 0.95);
}

.navbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem 2rem;
    background-color: var(--background-color);
    backdrop-filter: blur(10px);
    box-shadow: 0 4px 30px rgba(0, 0, 0, 0.1);
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    z-index: 1000;
    transition: all var(--transition-speed) ease;
}

.navbar:hover {
    box-shadow: 0 6px 40px rgba(0, 0, 0, 0.15);
}

.navbar-header {
    display: flex;
    align-items: center;
    gap: 3rem;
}

.logo {
    height: 40px;
    width: auto;
    transition: transform var(--transition-speed) ease;
}

.logo:hover {
    transform: scale(1.05);
}

.nav-links {
    display: flex;
    gap: 2rem;
    list-style: none;
    margin: 0;
    padding: 0;
}

.nav-links li {
    position: relative;
}

.nav-links a {
    text-decoration: none;
    color: var(--text-color);
    font-weight: 500;
    font-size: 0.9rem;
    letter-spacing: 1px;
    padding: 0.5rem 0;
    transition: all var(--transition-speed) ease;
    position: relative;
}

.nav-links a::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 0;
    height: 2px;
    background-color: var(--hover-color);
    transition: width var(--transition-speed) ease;
}

.nav-links a:hover {
    color: var(--hover-color);
}

.nav-links a:hover::after {
    width: 100%;
}

.profile-dropdown {
    position: relative;
}

.profile {
    display: flex;
    align-items: center;
    gap: 1rem;
    cursor: pointer;
    padding: 0.5rem 1rem;
    border-radius: 50px;
    transition: all var(--transition-speed) ease;
    background: transparent;
}

.profile:hover {
    background: rgba(0, 0, 0, 0.05);
}

.profile-img {
    width: 35px;
    height: 35px;
    border-radius: 50%;
    transition: transform var(--transition-speed) ease;
}

.profile-name {
    margin: 0;
    font-weight: 500;
}

.profile-dropdown-content {
    display: none;
    position: absolute;
    right: 0;
    top: 120%;
    background: var(--background-color);
    padding: 1rem;
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
    min-width: 150px;
    z-index: 1000;
}

.profile-dropdown-content.show {
    display: block;
}

.btn-danger {
    background-color: var(--hover-color);
    color: white;
    border: none;
    padding: 0.5rem 1.5rem;
    border-radius: 25px;
    cursor: pointer;
    transition: all var(--transition-speed) ease;
    width: 100%;
}

.btn-danger:hover {
    transform: translateY(-2px);
    box-shadow: 0 5px 15px rgba(231, 76, 60, 0.3);
}

/* Mobile User Section Styles */
.mobile-user-section {
    display: none;
    padding: 1rem;
    border-top: 1px solid rgba(0, 0, 0, 0.1);
    margin-top: 1rem;
}

.mobile-user-info {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-bottom: 1rem;
}

.mobile-profile-img {
    width: 40px;
    height: 40px;
    border-radius: 50%;
}

.mobile-user-name {
    font-weight: 500;
    color: var(--text-color);
}

.mobile-logout-btn {
    width: 100%;
    margin-top: 0.5rem;
}

/* Menu Toggle Styles */
.menu-toggle {
    display: none;
    cursor: pointer;
    padding: 8px;
    border-radius: 8px;
    transition: background-color 0.3s ease;
}

.menu-icon {
    width: 24px;
    height: 20px;
    position: relative;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
}

.menu-icon span {
    display: block;
    width: 100%;
    height: 2px;
    background-color: var(--text-color);
    border-radius: 2px;
    transition: all 0.3s ease;
}

/* Responsive Styles */
@media screen and (max-width: 968px) {
    .menu-toggle {
        display: block;
    }

    .menu-toggle:hover {
        background-color: rgba(0, 0, 0, 0.05);
    }

    .menu-toggle.active .menu-icon span:first-child {
        transform: translateY(9px) rotate(45deg);
    }

    .menu-toggle.active .menu-icon span:nth-child(2) {
        opacity: 0;
    }

    .menu-toggle.active .menu-icon span:last-child {
        transform: translateY(-9px) rotate(-45deg);
    }

    .nav-links {
        position: fixed;
        top: 70px;
        right: -100%;
        width: 250px;
        height: auto;
        max-height: calc(100vh - 70px);
        overflow-y: auto;
        background: var(--background-color);
        flex-direction: column;
        padding: 1rem;
        border-radius: 0 0 0 15px;
        box-shadow: -5px 5px 15px rgba(0, 0, 0, 0.1);
        transition: right 0.3s ease;
        opacity: 1;
    }

    .nav-links.active {
        right: 0;
    }

    .nav-links li {
        width: 100%;
        padding: 0.5rem 0;
    }

    .nav-links a {
        display: block;
        padding: 0.5rem 1rem;
        font-size: 0.9rem;
        border-radius: 8px;
        transition: all 0.3s ease;
    }

    .nav-links a:hover {
        background-color: rgba(0, 0, 0, 0.05);
    }

    .navbar-header {
        width: 100%;
        justify-content: space-between;
    }

    .profile-dropdown {
        display: none;
    }

    .mobile-user-section {
        display: block;
    }
}

@media screen and (max-width: 480px) {
    .navbar {
        padding: 0.5rem 1rem;
    }

    .logo {
        height: 30px;
    }

    .nav-links {
        width: 200px;
    }

    .nav-links a {
        font-size: 1rem;
    }
}
</style>

<!-- JavaScript -->
<script>
const menuToggle = document.querySelector('.menu-toggle');
const navLinks = document.querySelector('.nav-links');
const navbar = document.querySelector('.navbar');
const profileDropdown = document.querySelector('.profile');
const dropdownContent = document.querySelector('.profile-dropdown-content');

// Toggle menu for mobile
menuToggle.addEventListener('click', function() {
    this.classList.toggle('active');
    navLinks.classList.toggle('active');
});

// Toggle profile dropdown
profileDropdown?.addEventListener('click', (e) => {
    e.stopPropagation();
    dropdownContent.classList.toggle('show');
});

// Close dropdown when clicking outside
document.addEventListener('click', (e) => {
    if (profileDropdown && !profileDropdown.contains(e.target)) {
        dropdownContent.classList.remove('show');
    }
});

// Close menu when clicking outside
document.addEventListener('click', function(event) {
    if (!event.target.closest('.menu-toggle') && !event.target.closest('.nav-links')) {
        menuToggle.classList.remove('active');
        navLinks.classList.remove('active');
    }
});

// Scroll effect
let lastScroll = 0;
window.addEventListener('scroll', () => {
    const currentScroll = window.pageYOffset;
    
    if (currentScroll <= 0) {
        navbar.style.boxShadow = '0 4px 30px rgba(0, 0, 0, 0.1)';
        return;
    }
    
    if (currentScroll > lastScroll) {
        navbar.style.transform = 'translateY(-100%)';
    } else {
        navbar.style.transform = 'translateY(0)';
        navbar.style.boxShadow = '0 6px 40px rgba(0, 0, 0, 0.15)';
    }
    
    lastScroll = currentScroll;
});
</script>
// Football Simulator - Main JavaScript file

document.addEventListener('DOMContentLoaded', function () {
    initializeSimulator();
});

function initializeSimulator() {
    // Add any page-wide initializations here
    setupTooltips();
    setupNavigationHighlight();
}

function setupTooltips() {
    // Bootstrap tooltip initialization if needed
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

function setupNavigationHighlight() {
    // Highlight current nav item
    const currentPath = window.location.pathname;
    document.querySelectorAll('.nav-link, .navbar-nav a').forEach(link => {
        if (link.getAttribute('href') && link.getAttribute('href').includes(currentPath)) {
            link.classList.add('active');
        }
    });
}

// Matchday Simulation Functions
function simulateMatchday() {
    const btn = document.getElementById('simulateBtn');
    const originalHTML = btn.innerHTML;

    btn.disabled = true;
    btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Simulating...';

    fetch('/Simulation/SimulateMatchday', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        return response.json();
    })
    .then(data => {
        if (data.success) {
            handleSimulationResults(data, btn, originalHTML);
        } else {
            showError('Error simulating matchday: ' + (data.error || 'Unknown error'));
            btn.disabled = false;
            btn.innerHTML = originalHTML;
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showError('An error occurred during simulation: ' + error.message);
        btn.disabled = false;
        btn.innerHTML = originalHTML;
    });
}

function handleSimulationResults(data, btn, originalHTML) {
    const matches = data.matches;
    let simulatedCount = 0;

    // Animate each score reveal sequentially
    matches.forEach((match, index) => {
        setTimeout(() => {
            revealMatchScore(match, index, matches.length);
            simulatedCount++;
            updateRemainingMatches(matches.length - simulatedCount);
        }, index * 600); // Increased from 500ms to 600ms for better visibility
    });

    // After all reveals complete, update standings and handle next actions
    const finalDelay = matches.length * 600 + 1000;
    setTimeout(() => {
        updateStandingsTable();

        if (data.isSeasonComplete) {
            showSeasonCompletionMessage(btn);
        } else {
            enableNextMatchdayButton(btn);
        }
    }, finalDelay);
}

function revealMatchScore(match, index, totalMatches) {
    const scoreElement = document.getElementById(`score-${match.matchId}`);
    if (!scoreElement) return;

    const card = scoreElement.closest('.match-card');

    // Create the score display with animation
    const scoreHTML = `
        <div class="fs-2 fw-bold animate-score">
            ${match.homeGoals} <span class="text-muted">-</span> ${match.awayGoals}
        </div>
        <div class="mt-2">
            <span class="badge bg-${getResultBadgeClass(match.result)}">
                ${getResultText(match.result)}
            </span>
        </div>
    `;

    scoreElement.innerHTML = scoreHTML;
    card.classList.add('simulated');

    // Add animation pulse
    card.classList.add('pulse');
    setTimeout(() => card.classList.remove('pulse'), 600);

    // Play sound effect (optional - requires audio file)
    playScoreNotification();
}

function getResultBadgeClass(result) {
    switch (result) {
        case 'HomeWin': return 'success';
        case 'AwayWin': return 'danger';
        case 'Draw': return 'secondary';
        default: return 'dark';
    }
}

function getResultText(result) {
    switch (result) {
        case 'HomeWin': return 'Home Win';
        case 'AwayWin': return 'Away Win';
        case 'Draw': return 'Draw';
        default: return 'Unknown';
    }
}

function updateRemainingMatches(remaining) {
    const elem = document.getElementById('remainingMatches');
    if (elem) {
        elem.textContent = Math.max(0, remaining);
    }
}

function updateStandingsTable() {
    fetch('/Simulation/GetStandings')
        .then(response => response.json())
        .then(standings => {
            console.log('Standings updated:', standings);
            // Could update a standings display here if visible
        })
        .catch(error => console.error('Error updating standings:', error));
}

function enableNextMatchdayButton(btn) {
    setTimeout(() => {
        btn.innerHTML = '<i class="fa fa-arrow-right"></i> Next Matchday';
        btn.id = 'nextMatchdayBtn';
        btn.onclick = nextMatchday;
        btn.disabled = false;
    }, 500);
}

function showSeasonCompletionMessage(btn) {
    const modal = createModal('Season Complete!', 
        'All matchdays have been simulated. Your season is now complete! Check the final standings below.',
        [
            { text: 'View Final Standings', action: () => window.location.href = '/Simulation/Standings', class: 'btn-primary' },
            { text: 'Start New Season', action: () => window.location.href = '/Simulation/StartSeason', class: 'btn-success' },
            { text: 'Return to Dashboard', action: () => window.location.href = '/Simulation', class: 'btn-secondary' }
        ]
    );
    modal.show();
}

function nextMatchday() {
    window.location.href = '/Simulation/Matchday';
}

function showError(message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-danger alert-dismissible fade show';
    alertDiv.role = 'alert';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    const container = document.querySelector('.container-fluid') || document.body;
    container.insertBefore(alertDiv, container.firstChild);

    setTimeout(() => {
        alertDiv.classList.remove('show');
        setTimeout(() => alertDiv.remove(), 150);
    }, 5000);
}

function playScoreNotification() {
    // Optional: Add notification sound
    // You can uncomment this if you have an audio file
    // const audio = new Audio('/sounds/notification.mp3');
    // audio.play().catch(e => console.log('Audio play failed:', e));
}

function createModal(title, body, buttons) {
    const modalId = 'modal-' + Date.now();

    const buttonsHTML = buttons.map(btn => 
        `<button type="button" class="btn ${btn.class}" onclick="${btn.action.toString().split('=>')[1] || 'this.closest(\".modal\").style.display=\"none\"'}">${btn.text}</button>`
    ).join('');

    const modalHTML = `
        <div class="modal fade" id="${modalId}" tabindex="-1" aria-labelledby="modalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="modalLabel">${title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        ${body}
                    </div>
                    <div class="modal-footer">
                        ${buttonsHTML}
                    </div>
                </div>
            </div>
        </div>
    `;

    document.body.insertAdjacentHTML('beforeend', modalHTML);
    return new bootstrap.Modal(document.getElementById(modalId));
}

// Utility Functions
function formatNumber(num) {
    return new Intl.NumberFormat('en-US').format(num);
}

function getTeamColor(r, g, b) {
    return `rgb(${r}, ${g}, ${b})`;
}

// Export functions for inline onclick handlers
window.simulateMatchday = simulateMatchday;
window.nextMatchday = nextMatchday;
window.formatNumber = formatNumber;

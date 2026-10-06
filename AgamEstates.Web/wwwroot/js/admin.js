/**
 * Agam Estates - Admin CRM Client Utilities
 */

// Toast notification helper
function showToast(message, type = 'success') {
  let container = document.getElementById('toast-container');
  if (!container) {
    container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container';
    document.body.appendChild(container);
  }

  const toast = document.createElement('div');
  toast.className = `toast ${type === 'error' ? 'toast-error' : type === 'info' ? 'toast-info' : ''}`;
  
  let iconSvg = '';
  if (type === 'error') {
    iconSvg = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>';
  } else {
    iconSvg = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="20 6 9 17 4 12"></polyline></svg>';
  }

  toast.innerHTML = `
    <span class="toast-icon">${iconSvg}</span>
    <span class="toast-message">${message}</span>
    <button class="toast-close" onclick="this.parentElement.remove()">&times;</button>
  `;

  container.appendChild(toast);

  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transform = 'translateY(-10px)';
    setTimeout(() => toast.remove(), 300);
  }, 4000);
}

// Modal open/close helpers
function openModal(modalId) {
  const modal = document.getElementById(modalId);
  if (modal) {
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
  }
}

function closeModal(modalId) {
  const modal = document.getElementById(modalId);
  if (modal) {
    modal.classList.remove('active');
    document.body.style.overflow = '';
  }
}

// Format Indian Currency
function formatIndianCurrency(num) {
  if (num === null || num === undefined || isNaN(num)) return '—';
  const val = Number(num);
  return '₹ ' + val.toLocaleString('en-IN', { maximumFractionDigits: 0 });
}

// Sidebar Profile Popover
function toggleProfilePopover(e) {
  if (e) {
    e.stopPropagation();
  }
  const popover = document.getElementById('sidebar-profile-popover');
  const trigger = document.getElementById('sidebar-profile-trigger');
  if (!popover || !trigger) return;

  const isOpen = popover.classList.contains('open');
  if (isOpen) {
    closeProfilePopover();
  } else {
    openProfilePopover();
  }
}

function openProfilePopover() {
  const popover = document.getElementById('sidebar-profile-popover');
  const trigger = document.getElementById('sidebar-profile-trigger');
  if (popover && trigger) {
    popover.classList.add('open');
    trigger.classList.add('active');
    trigger.setAttribute('aria-expanded', 'true');
  }
}

function closeProfilePopover() {
  const popover = document.getElementById('sidebar-profile-popover');
  const trigger = document.getElementById('sidebar-profile-trigger');
  if (popover && trigger) {
    popover.classList.remove('open');
    trigger.classList.remove('active');
    trigger.setAttribute('aria-expanded', 'false');
  }
}

function viewDetailedProfile() {
  closeProfilePopover();
  openModal('my-account-modal');
}

// Mobile sidebar toggle
document.addEventListener('DOMContentLoaded', () => {
  const sidebar = document.getElementById('admin-sidebar');
  const toggleBtn = document.getElementById('sidebar-toggle-btn');
  const closeBackdrop = document.getElementById('sidebar-backdrop');
  const profileTrigger = document.getElementById('sidebar-profile-trigger');

  // Keyboard accessibility for sidebar profile row
  if (profileTrigger) {
    profileTrigger.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        toggleProfilePopover(e);
      }
    });
  }

  // Close profile popover when clicking outside
  document.addEventListener('click', (e) => {
    const popover = document.getElementById('sidebar-profile-popover');
    const trigger = document.getElementById('sidebar-profile-trigger');
    if (popover && popover.classList.contains('open')) {
      if (!popover.contains(e.target) && !trigger.contains(e.target)) {
        closeProfilePopover();
      }
    }
  });

  if (toggleBtn && sidebar) {
    toggleBtn.addEventListener('click', () => {
      sidebar.classList.toggle('open');
      if (closeBackdrop) closeBackdrop.classList.toggle('active');
    });
  }

  if (closeBackdrop && sidebar) {
    closeBackdrop.addEventListener('click', () => {
      sidebar.classList.remove('open');
      closeBackdrop.classList.remove('active');
    });
  }

  // Close modals on clicking backdrop
  document.querySelectorAll('.modal-backdrop').forEach(backdrop => {
    backdrop.addEventListener('click', (e) => {
      if (e.target === backdrop) {
        backdrop.classList.remove('active');
        document.body.style.overflow = '';
      }
    });
  });

  // Close popover, modals, and mobile sidebar on Escape key
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      closeProfilePopover();
      document.querySelectorAll('.modal-backdrop.active').forEach(modal => {
        modal.classList.remove('active');
      });
      document.body.style.overflow = '';
      if (sidebar && sidebar.classList.contains('open')) {
        sidebar.classList.remove('open');
        if (closeBackdrop) closeBackdrop.classList.remove('active');
      }
    }
  });

  // Auto-close sidebar on link click on mobile viewports
  if (sidebar) {
    sidebar.querySelectorAll('.sidebar-link').forEach(link => {
      link.addEventListener('click', () => {
        closeProfilePopover();
        if (window.innerWidth <= 992) {
          sidebar.classList.remove('open');
          if (closeBackdrop) closeBackdrop.classList.remove('active');
        }
      });
    });
  }
});

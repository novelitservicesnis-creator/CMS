(() => {
    const form = document.getElementById('enquiry-form');
    if (!form) return;
    form.removeAttribute('novalidate');
    const phone = form.elements.phoneNumber;
    phone.addEventListener('input', () => phone.setCustomValidity(''));
    form.addEventListener('submit', async event => {
        event.preventDefault();
        phone.setCustomValidity(/^[0-9+() -]{7,20}$/.test(phone.value.trim()) ? '' : 'Please enter a valid phone number.');
        if (!form.reportValidity()) return;
        const button = form.querySelector('.btn-enquiry-submit');
        const text = form.querySelector('.btn-text');
        const spinner = form.querySelector('.btn-spinner');
        const status = document.getElementById('contact-form-msg');
        const original = text.textContent;
        button.disabled = true;
        text.textContent = 'Sending...';
        spinner.style.display = 'inline-block';
        status.textContent = '';
        status.style.display = 'none';
        try {
            const payload = Object.fromEntries(new FormData(form));
            Object.keys(payload).forEach(key => payload[key] = payload[key].trim());
            if (!payload.name) throw new Error('Please enter your name.');
            const response = await fetch('/Home/SubmitEnquiry', {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
            });
            if (!response.ok) throw new Error('Unable to send your enquiry. Please try again.');
            const result = await response.json();
            if (!result.success) throw new Error('Unable to send your enquiry. Please check your details and try again.');
            status.textContent = result.message || 'Thank you. Our team will contact you shortly.';
            status.style.color = 'var(--primary)';
            form.reset();
        } catch (error) {
            status.textContent = error.message || 'Unable to send your enquiry. Please try again.';
            status.style.color = '#b42318';
        } finally {
            status.style.display = 'block';
            button.disabled = false;
            text.textContent = original;
            spinner.style.display = 'none';
        }
    });
})();

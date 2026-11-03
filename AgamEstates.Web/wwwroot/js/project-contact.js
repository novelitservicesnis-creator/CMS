(() => {
    const form = document.getElementById('enquiry-form');
    if (!form) return;
    form.removeAttribute('novalidate');
    const phone = form.elements.phoneNumber;
    const status = form.querySelector('.form-msg');
    let submitting = false;
    const clearStatus = () => {
        status.hidden = true;
        status.textContent = '';
        status.classList.remove('show');
    };
    phone.addEventListener('input', () => phone.setCustomValidity(''));
    form.addEventListener('input', clearStatus);
    form.addEventListener('change', clearStatus);
    form.addEventListener('submit', async event => {
        event.preventDefault();
        if (submitting) return;
        clearStatus();
        phone.setCustomValidity(/^[0-9+() -]{7,20}$/.test(phone.value.trim()) ? '' : 'Please enter a valid phone number.');
        if (!form.reportValidity()) return;
        const button = form.querySelector('.btn-enquiry-submit');
        const text = form.querySelector('.btn-text');
        const spinner = form.querySelector('.btn-spinner');
        const original = text.textContent;
        submitting = true;
        button.disabled = true;
        text.textContent = 'Sending...';
        spinner.style.display = 'inline-block';
        try {
            const payload = Object.fromEntries(new FormData(form));
            Object.keys(payload).forEach(key => payload[key] = payload[key].trim());
            if (!payload.name) throw new Error('Please enter your name.');
            const response = await fetch('/Home/SubmitEnquiry', {
                method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload)
            });
            if (!response.ok) throw new Error('Unable to send your enquiry. Please try again.');
            const result = await response.json();
            if (result?.success !== true) throw new Error(result?.message || 'Unable to send your enquiry. Please check your details and try again.');
            status.textContent = 'Thank you — our sales team will call you back shortly.';
            status.style.color = 'var(--primary)';
            form.reset();
        } catch (error) {
            status.textContent = error instanceof TypeError || error instanceof SyntaxError
                ? 'Unable to send your enquiry. Please try again.'
                : (error.message || 'Unable to send your enquiry. Please try again.');
            status.style.color = '#b42318';
        } finally {
            status.hidden = false;
            status.classList.add('show');
            submitting = false;
            button.disabled = false;
            text.textContent = original;
            spinner.style.display = 'none';
        }
    });
})();

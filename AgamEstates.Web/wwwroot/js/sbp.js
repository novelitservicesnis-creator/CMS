(() => {
    const interest = document.querySelector('#popup-enquiry-form [name="propertyInterest"]');
    const options = ['SBP Leroy Towers — Please advise', 'SBP Leroy Towers — 2 BHK', 'SBP Leroy Towers — 3+1 BHK'];
    if (interest) interest.replaceChildren(...options.map((name, i) => new Option(name, name, i === 0, i === 0)));
    document.querySelectorAll('[data-sbp-enquiry]').forEach(button => {
        button.addEventListener('click', () => {
            if (interest) interest.value = button.dataset.sbpEnquiry;
            openEnquiryModal();
        });
    });
})();

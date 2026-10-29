(() => {
    const interest = document.querySelector('#popup-enquiry-form [name="propertyInterest"]');
    const options = ['Bollywood Greens — Please advise', 'Bollywood Greens — 3 BHK', 'Bollywood Greens — 4 BHK'];
    if (interest) interest.replaceChildren(...options.map((name, i) => new Option(name, name, i === 0, i === 0)));
    document.querySelectorAll('[data-bgc-enquiry]').forEach(button => {
        button.addEventListener('click', () => {
            if (interest) interest.value = button.dataset.bgcEnquiry;
            openEnquiryModal();
        });
    });
})();

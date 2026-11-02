(() => {
    const interest = document.querySelector('#popup-enquiry-form [name="propertyInterest"]');
    const options = ['Jubilee — Please advise', 'Jubilee Parkfield', 'Jubilee Denvox — Office space', 'Jubilee Avelia — S+4 floors', 'Jubilee Elvira — 3 BHK', 'Jubilee Elvira — 3+1 BHK', 'Jubilee Mercasa — Residential', 'Jubilee Avenoir — Commercial'];
    if (interest) interest.replaceChildren(...options.map((name, i) => new Option(name, name, i === 0, i === 0)));
    document.querySelectorAll('[data-jubilee-enquiry]').forEach(button => {
        button.addEventListener('click', () => {
            if (interest) interest.value = button.dataset.jubileeEnquiry;
            openEnquiryModal();
        });
    });
})();

(() => {
    const interest = document.querySelector('#popup-enquiry-form [name="propertyInterest"]');
    const projects = ['Noble — Please advise', 'Noble Aurellia', 'Noble Aurellia — 3 BHK', 'Noble Aurellia — 3+1 BHK', 'Noble Magnollia', 'Noble Magnollia — 3+1 BHK MAX', 'Noble Magnollia — 4+1 BHK', 'Noble Magnollia — 5+1 BHK'];
    if (interest) interest.replaceChildren(...projects.map((name, i) => new Option(name, name, i === 0, i === 0)));
    document.querySelectorAll('[data-noble-enquiry]').forEach(button => {
        button.addEventListener('click', () => {
            if (interest) interest.value = button.dataset.nobleEnquiry;
            openEnquiryModal();
        });
    });
})();

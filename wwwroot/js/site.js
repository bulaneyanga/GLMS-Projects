(() => {
  const amount = document.querySelector('#AmountUsd');
  const output = document.querySelector('#zarPreview');
  const rateElement = document.querySelector('[data-exchange-rate]');
  if (!amount || !output || !rateElement) return;
  const rate = Number(rateElement.dataset.exchangeRate);
  const update = () => {
    const usd = Number(amount.value);
    output.textContent = Number.isFinite(usd) && usd >= 0 && rate > 0
      ? new Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' }).format(usd * rate)
      : 'Enter a USD amount';
  };
  amount.addEventListener('input', update);
  update();
})();

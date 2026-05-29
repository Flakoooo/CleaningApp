function normalizeDigits(input) {
    let digits = input.replace(/\D/g, '').substring(0, 11);
    if (digits.length > 0) {
        if (digits[0] === '8') digits = '7' + digits.substring(1);
        if (digits[0] !== '7') digits = '7' + digits;
    }
    return digits;
}

function formatPhone(digits) {
    if (!digits || digits.length === 0) return '';

    let result = '+7';
    if (digits.length > 1) result += ' (' + digits.substring(1, Math.min(4, digits.length));
    if (digits.length >= 5) result += ') ' + digits.substring(4, Math.min(7, digits.length));
    if (digits.length >= 8) result += '-' + digits.substring(7, Math.min(9, digits.length));
    if (digits.length >= 10) result += '-' + digits.substring(9, 11);

    return result;
}

function calculateCursorPosition(oldPos, oldFormatted, newFormatted) {
    if (oldPos >= oldFormatted.length) return newFormatted.length;

    let shift = 0;
    if (oldFormatted.length >= 3 && newFormatted.length >= 4 && oldFormatted[2] !== '(' && newFormatted[2] === '(') {
        if (oldPos >= 3) shift++;
    }
    if (oldFormatted.length >= 7 && newFormatted.length >= 8 && oldFormatted[6] !== ')' && newFormatted[6] === ')') {
        if (oldPos >= 7) shift++;
    }
    if (oldFormatted.length >= 10 && newFormatted.length >= 11 && oldFormatted[9] !== '-' && newFormatted[9] === '-') {
        if (oldPos >= 10) shift++;
    }
    if (oldFormatted.length >= 13 && newFormatted.length >= 14 && oldFormatted[12] !== '-' && newFormatted[12] === '-') {
        if (oldPos >= 13) shift++;
    }

    return Math.min(oldPos + shift, newFormatted.length);
}

function initPhoneMask(dotNetHelper, inputElement) {
    if (!inputElement) return;

    inputElement.addEventListener('input', function (e) {
        const oldValue = inputElement.value;
        const oldCursor = inputElement.selectionStart;

        const rawDigits = normalizeDigits(oldValue);

        const newFormatted = formatPhone(rawDigits);

        inputElement.value = newFormatted;

        const newCursor = calculateCursorPosition(oldCursor, oldValue, newFormatted);
        inputElement.setSelectionRange(newCursor, newCursor);

        dotNetHelper.invokeMethodAsync('UpdateRawDigits', rawDigits);
    });

    if (inputElement.value) {
        const initialRaw = normalizeDigits(inputElement.value);
        const initialFormatted = formatPhone(initialRaw);
        inputElement.value = initialFormatted;
        dotNetHelper.invokeMethodAsync('UpdateRawDigits', initialRaw);
    }
}

function setPhoneValue(inputElement, rawDigits) {
    if (!inputElement) return;

    const normalized = normalizeDigits(rawDigits);
    const formatted = formatPhone(normalized);
    inputElement.value = formatted;
}
const assert = require('node:assert/strict');
const { test } = require('node:test');
const vm = require('node:vm');
const fs = require('node:fs');
const path = require('node:path');

function setup() {
    const listeners = new Map(); const revoked = []; let sequence = 0;
    const name = { dataset: {}, textContent: 'Selecteer afbeelding' };
    class FakeImage {
        dataset = { url: 'javascript:untrusted-dom-value' }; hidden = true;
        removeAttribute(key) { if (key === 'src') delete this.src; }
    }
    const preview = new FakeImage();
    const nodes = { '[data-file-name]': name, '[data-image-error]': { hidden: true }, '[data-image-preview]': preview };
    const form = { dirty: false, querySelector: selector => nodes[selector], toggleAttribute(key, value) { if (key === 'data-unsaved') this.dirty = value; } };
    class FakeInput {
        files = []; form = form;
        closest(selector) { return selector.startsWith('form[data-choir-profile]') ? null : form; }
        matches(selector) { return selector === '[data-choir-file]'; }
    }
    const bar = { hidden: true };
    const document = {
        addEventListener(type, listener) { listeners.set(type, listener); },
        querySelectorAll() { return []; },
        querySelector(selector) { return selector === '[data-image-save-bar]' ? bar : form.dirty ? form : null; }
    };
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, '../../Amusing.Mijn/wwwroot/choir-file.js'), 'utf8'), {
        document, HTMLInputElement: FakeInput, HTMLImageElement: FakeImage,
        window: { addEventListener() {}, Blazor: null },
        URL: { createObjectURL() { return 'blob:local-test/' + (++sequence); }, revokeObjectURL(url) { revoked.push(url); } },
        WeakMap, JSON, FormData: class {}, Event: class {}
    });
    const input = new FakeInput();
    return { preview, name, revoked, setPreview(element) { nodes['[data-image-preview]'] = element; }, choose(file) { input.files = file ? [file] : []; listeners.get('change')({ target: input }); } };
}

test('preview uses its generated object URL, never a URL read from the DOM', () => {
    const fixture = setup(); fixture.choose({ name: 'foto.png', type: 'image/png' });
    assert.equal(fixture.preview.src, 'blob:local-test/1');
    assert.equal(fixture.preview.dataset.url, 'javascript:untrusted-dom-value');
    assert.deepEqual(fixture.revoked, []);
});
test('replacing and clearing previews revokes only object URLs owned by this script', () => {
    const fixture = setup(); fixture.choose({ name: 'one.png', type: 'image/png' });
    fixture.preview.dataset.url = 'https://example.invalid/untrusted';
    fixture.choose({ name: 'two.webp', type: 'image/webp' }); fixture.choose(null);
    assert.deepEqual(fixture.revoked, ['blob:local-test/1', 'blob:local-test/2']);
    assert.equal(fixture.preview.hidden, true); assert.equal(fixture.preview.src, undefined);
});
test('filename markup stays plain text and unsupported formats do not get a preview', () => {
    const fixture = setup(); const name = '<img src=x onerror=alert(1)>.png';
    fixture.choose({ name, type: 'image/png' }); assert.equal(fixture.name.textContent, name);
    fixture.choose({ name: 'script.svg', type: 'image/svg+xml' });
    assert.equal(fixture.preview.src, undefined); assert.equal(fixture.preview.hidden, true);
});

test('a non-image element cannot become a preview URL sink', () => {
    const fixture = setup(); const element = { dataset: {}, hidden: true };
    fixture.setPreview(element); fixture.choose({ name: 'photo.png', type: 'image/png' });
    assert.equal(element.src, undefined);
});

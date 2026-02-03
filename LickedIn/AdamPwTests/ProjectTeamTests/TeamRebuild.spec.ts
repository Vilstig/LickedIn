import { test, expect } from '@playwright/test';

test.use({
    storageState: 'auth.json'
});

test('test', async ({ page }) => {
    await page.goto('http://localhost:5176/');
    await page.getByRole('link', { name: 'Zarządzaj Projektami' }).click();
    await page.getByRole('link', { name: ' Zespół' }).first().click();
    page.once('dialog', dialog => {
        console.log(`Dialog message: ${dialog.message()}`);
        dialog.accept().catch(() => { });
    });
    await page.locator('button[title="Zwolnij miejsce"]').first().click();
    await page.getByRole('button', { name: ' Automatycznie uzupełnij' }).click();
});
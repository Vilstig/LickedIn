import { test, expect } from '@playwright/test';

test.use({
    storageState: 'auth.json'
});

test('test', async ({ page }) => {
    await page.goto('http://localhost:5176/');
    await page.getByRole('link', { name: 'Zarządzaj Projektami' }).click();
    await page.getByTitle('Usuń').first().click();
    await page.getByRole('button', { name: 'Tak, usuń projekt' }).click();
});
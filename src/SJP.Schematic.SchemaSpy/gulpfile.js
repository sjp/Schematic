'use strict';

const gulp = require('gulp');
const rename = require('gulp-rename');
const eslint = require('gulp-eslint');
const uglify = require('gulp-uglify');
const concat = require('gulp-concat');
const sass = require('gulp-sass');
const cssnano = require('gulp-cssnano');
const postcss = require('gulp-postcss');
const cssnext = require('postcss-cssnext');
const del = require('del');

gulp.task('clean', function () {
    return del([
        'assets/**/*.*'
    ]);
});

gulp.task('fonts', function () {
    return gulp.src('node_modules/source-sans-pro/**/*.{eot,woff,woff2,otf,ttf}')
        .pipe(gulp.dest('assets/css/fonts'));
});

gulp.task('styles:dev', ['fonts'], function () {
    return gulp.src(
        [
            'node_modules/bootstrap/dist/css/bootstrap.css',
            'node_modules/font-awesome/css/font-awesome.css',
            'node_modules/ionicons/dist/css/ionicons.css',
            'node_modules/datatables.net-bs/css/dataTables.bootstrap.css',
            'node_modules/datatables.net-buttons-bs/css/buttons.bootstrap.css',
            'node_modules/codemirror/lib/codemirror.css',
            'node_modules/admin-lte/dist/css/AdminLTE.css',
            'node_modules/admin-lte/dist/css/skins/_all-skins.css',
            'Source/css/source-sans-pro.css'
            // TODO: add the schemaSpy.css file
        ])
        .pipe(concat('schemaspy-app.css'))
        .pipe(postcss([cssnext]))
        .pipe(gulp.dest("assets/"));
});

gulp.task('scripts:dev', function () {
    return gulp.src(
        [
            'node_modules/jquery/dist/jquery.js',
            'node_modules/jquery-ui-dist/jquery-ui.js',
            'node_modules/bootstrap/dist/js/bootstrap.js',
            'node_modules/datatables.net/js/jquery.dataTables.js',
            'node_modules/datatables.net-bs/js/dataTables.bootstrap.js',
            'node_modules/datatables.net-buttons/js/dataTables.buttons.js',
            'node_modules/datatables.net-buttons-bs/js/buttons.bootstrap.js',
            'node_modules/datatables.net-buttons/js/buttons.html5.js',
            'node_modules/datatables.net-buttons/js/buttons.print.js',
            'node_modules/datatables.net-buttons/js/buttons.colVis.js',
            'node_modules/salvattore/dist/salvattore.js',
            'node_modules/anchor-js/anchor.js',
            'node_modules/codemirror/lib/codemirror.js',
            'node_modules/codemirror/mode/sql/sql.js',
            'node_modules/admin-lte/dist/js/adminlte.js'
            // TODO: add the schemaSpy.js file
        ])
        .pipe(concat('schemaspy-app.js'))
        .pipe(eslint())
        .pipe(gulp.dest("assets/"));
});

gulp.task('build:dev', ['styles:dev', 'scripts:dev'], function () {

});

gulp.task('styles:prod', ['fonts'], function () {
    return gulp.src(
        [
            'node_modules/bootstrap/dist/css/bootstrap.min.css',
            'node_modules/font-awesome/css/font-awesome.min.css',
            'node_modules/ionicons/dist/css/ionicons.min.css',
            'node_modules/datatables.net-bs/css/dataTables.bootstrap.min.css',
            'node_modules/datatables.net-buttons-bs/css/buttons.bootstrap.min.css',
            'node_modules/codemirror/lib/codemirror.css',
            'node_modules/admin-lte/dist/css/AdminLTE.min.css',
            'node_modules/admin-lte/dist/css/skins/_all-skins.min.css',
            'Source/css/source-sans-pro.css'
            // TODO: add the schemaSpy.css file
        ])
        .pipe(concat('schemaspy-app.css'))
        .pipe(postcss([cssnext]))
        .pipe(cssnano())
        .pipe(gulp.dest("assets/css"));
});

gulp.task('scripts:prod', function () {
    return gulp.src(
        [
            'node_modules/jquery/dist/jquery.min.js',
            'node_modules/jquery-ui-dist/jquery-ui.min.js',
            'node_modules/bootstrap/dist/js/bootstrap.min.js',
            'node_modules/datatables.net/js/jquery.dataTables.js',
            'node_modules/datatables.net-bs/js/dataTables.bootstrap.js',
            'node_modules/datatables.net-buttons/js/dataTables.buttons.min.js',
            'node_modules/datatables.net-buttons-bs/js/buttons.bootstrap.min.js',
            'node_modules/datatables.net-buttons/js/buttons.html5.min.js',
            'node_modules/datatables.net-buttons/js/buttons.print.min.js',
            'node_modules/datatables.net-buttons/js/buttons.colVis.min.js',
            'node_modules/salvattore/dist/salvattore.min.js',
            'node_modules/anchor-js/anchor.min.js',
            'node_modules/codemirror/lib/codemirror.js',
            'node_modules/codemirror/mode/sql/sql.js',
            'node_modules/admin-lte/dist/js/adminlte.min.js'
            // TODO: add the schemaSpy.js file
        ])
        .pipe(concat('schemaspy-app.js'))
        .pipe(eslint())
        .pipe(uglify())
        .pipe(gulp.dest("assets/js"));
});

gulp.task('build:prod', ['styles:prod', 'scripts:prod'], function () {

});

gulp.task('default', ['build:prod'], function () {

});
const path = require("path");
const HtmlWebpackPlugin = require("html-webpack-plugin");

module.exports = {
    entry: "./src/index",
    mode: "development",
    devServer: {
        port: 10010,
        contentBase: path.join(__dirname, "dist"),
    },
    output: {
        publicPath: "auto",
    },
    target: ['web', 'es5'],
    module: {
        rules: [
            {
                test: /\.jsx?$/,
                loader: "babel-loader",
                exclude: /node_modules/,
            }, {
                test: /\.css$/i,
                use: ["style-loader", "css-loader"],
            }, {
                test: /\.less$/i,
                use: [
                    "style-loader",
                    "css-loader",
                    "less-loader",
                ],
            }, {
                test: /\.svg(\?v=\d+\.\d+\.\d+)?$/,
                use: [
                    {
                        loader: 'babel-loader',
                    }, {
                        loader: '@svgr/webpack',
                        options: {
                            babel: false,
                            icon: true,
                        },
                    },
                ],
            }
        ],
    },
    resolve: {
        alias: {
            '@': path.resolve(__dirname, 'src', 'core'),
            'models': path.resolve(__dirname, 'src', 'models'),
        }
    },
    plugins: [
        new HtmlWebpackPlugin({
            template: "./public/index.html",
        }),
    ],
};
